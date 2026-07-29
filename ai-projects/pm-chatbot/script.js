import { pipeline, env } from "https://esm.run/@huggingface/transformers";

// Always fetch models from the Hugging Face CDN, never look for local files
env.allowLocalModels = false;

const EMBED_MODEL_ID = "Xenova/bge-small-en-v1.5";
const GEN_MODEL_ID = "onnx-community/gemma-3-270m-it-ONNX";
const TOP_K = 4;
const SIMILARITY_THRESHOLD = 0.45; // tune this if the guardrail feels too strict/loose

let generator = null;
let embedder = null;
let kbChunks = [];

const chatLog = document.getElementById("chat-log");
const chatForm = document.getElementById("chat-form");
const chatInput = document.getElementById("chat-input");
const sendButton = chatForm.querySelector("button");
const statusEl = document.getElementById("status");
const progressTrack = document.getElementById("progress-track");
const progressBar = document.getElementById("progress-bar");

async function init() {
  try {
    setStatus("Loading knowledge base...");
    const res = await fetch("data/kb-embeddings.json");
    if (!res.ok) throw new Error("Could not load data/kb-embeddings.json");
    kbChunks = await res.json();

    setStatus("Loading embedding model...");
    embedder = await pipeline("feature-extraction", EMBED_MODEL_ID);

    setStatus("Loading language model (runs on CPU, first visit takes a bit)...");
    progressTrack.classList.remove("hidden");
    generator = await pipeline("text-generation", GEN_MODEL_ID, {
      dtype: "q8",
      progress_callback: (p) => {
        if (p.status === "progress" && p.total) {
          const pct = Math.round((p.loaded / p.total) * 100);
          progressBar.style.width = `${pct}%`;
          setStatus(`Loading language model... ${pct}%`);
        }
      },
    });

    progressTrack.classList.add("hidden");
    setStatus(null);
    chatInput.disabled = false;
    chatInput.placeholder = "Ask a project management question...";
    sendButton.disabled = false;
  } catch (err) {
    console.error(err);
    setStatus("Failed to load the assistant. Check the console and try refreshing.");
  }
}

function cosineSimilarity(a, b) {
  let dot = 0, normA = 0, normB = 0;
  for (let i = 0; i < a.length; i++) {
    dot += a[i] * b[i];
    normA += a[i] * a[i];
    normB += b[i] * b[i];
  }
  return dot / (Math.sqrt(normA) * Math.sqrt(normB));
}

async function embedQuery(text) {
  const output = await embedder(text, { pooling: "mean", normalize: true });
  return Array.from(output.data);
}

function retrieveTopChunks(queryEmbedding, k = TOP_K) {
  const scored = kbChunks.map((c) => ({
    ...c,
    score: cosineSimilarity(queryEmbedding, c.embedding),
  }));
  scored.sort((a, b) => b.score - a.score);
  return scored.slice(0, k);
}

function buildPrompt(query, chunks) {
  const context = chunks.map((c) => `[${c.title}]\n${c.text}`).join("\n\n");
  return `Answer the question using ONLY the context below. If the context doesn't contain enough information to answer, say so honestly instead of guessing. Be concise.

Context:
${context}

Question: ${query}`;
}

chatForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const question = chatInput.value.trim();
  if (!question || !generator) return;

  appendMessage("user", question);
  chatInput.value = "";
  setStatus("Thinking...");

  const queryEmbedding = await embedQuery(question);
  const topChunks = retrieveTopChunks(queryEmbedding);

  // Guardrail: if even the best-matching chunk isn't close enough, treat
  // the question as off-topic rather than letting the model guess.
  if (topChunks.length === 0 || topChunks[0].score < SIMILARITY_THRESHOLD) {
    appendMessage(
      "bot",
      "I'm focused on project management topics — Agile, Scrum, risk, scheduling, stakeholders, and related areas. Could you rephrase your question around one of those?"
    );
    setStatus(null);
    return;
  }

  const prompt = buildPrompt(question, topChunks);

  const output = await generator(
    [{ role: "user", content: prompt }],
    { max_new_tokens: 300, temperature: 0.3, do_sample: false }
  );

  const generated = output[0].generated_text;
  const answer = Array.isArray(generated)
    ? generated[generated.length - 1].content.trim()
    : String(generated).trim();

  const seen = new Set();
  const sources = [];
  for (const c of topChunks) {
    const key = `${c.title}|${c.source_url}`;
    if (!seen.has(key)) {
      seen.add(key);
      sources.push({ title: c.title, url: c.source_url });
    }
  }

  appendMessage("bot", answer, sources);
  setStatus(null);
});

function appendMessage(role, text, sources) {
  const el = document.createElement("div");
  el.className = `message ${role}`;
  el.textContent = text;

  if (sources && sources.length > 0) {
    const sourcesEl = document.createElement("div");
    sourcesEl.className = "sources";
    sourcesEl.innerHTML =
      "Sources: " +
      sources
        .map((s) => `<a href="${s.url}" target="_blank" rel="noopener">${s.title}</a>`)
        .join(", ");
    el.appendChild(sourcesEl);
  }

  chatLog.appendChild(el);
  chatLog.scrollTop = chatLog.scrollHeight;
}

function setStatus(text) {
  if (!text) {
    statusEl.classList.add("hidden");
    return;
  }
  statusEl.classList.remove("hidden");
  statusEl.textContent = text;
}

init();
