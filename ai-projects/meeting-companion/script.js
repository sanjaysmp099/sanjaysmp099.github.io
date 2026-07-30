import { pipeline, env } from "https://esm.run/@huggingface/transformers";

// Always fetch models from the Hugging Face CDN, never look for local files
env.allowLocalModels = false;

const ASR_MODEL_ID = "Xenova/whisper-tiny.en";
const SUMMARY_MODEL_ID = "onnx-community/gemma-3-270m-it-ONNX";

let transcriber = null;
let summarizer = null;
let currentFileURL = null;

const audioInput = document.getElementById("audio-input");
const runButton = document.getElementById("run-button");
const transcriptBox = document.getElementById("transcript-box");
const summaryBox = document.getElementById("summary-box");
const statusEl = document.getElementById("status");
const progressTrack = document.getElementById("progress-track");
const progressBar = document.getElementById("progress-bar");

async function init() {
  try {
    setStatus("Loading speech-to-text model...");
    progressTrack.classList.remove("hidden");

    transcriber = await pipeline("automatic-speech-recognition", ASR_MODEL_ID, {
      dtype: "fp32",
      progress_callback: (p) => reportProgress(p, "Loading speech-to-text model..."),
    });

    setStatus("Loading summarization model...");
    summarizer = await pipeline("text-generation", SUMMARY_MODEL_ID, {
      dtype: "fp32",
      progress_callback: (p) => reportProgress(p, "Loading summarization model..."),
    });

    progressTrack.classList.add("hidden");
    setStatus("Ready. Upload a meeting recording to begin.");
    audioInput.disabled = false;
  } catch (err) {
    console.error(err);
    setStatus("Failed to load the app. Check the console and try refreshing.");
  }
}

function reportProgress(p, label) {
  if (p.status === "progress" && p.total) {
    const pct = Math.round((p.loaded / p.total) * 100);
    progressBar.style.width = `${pct}%`;
    setStatus(`${label} ${pct}%`);
  }
}

audioInput.addEventListener("change", () => {
  if (currentFileURL) URL.revokeObjectURL(currentFileURL);

  const file = audioInput.files[0];
  if (!file) {
    runButton.disabled = true;
    return;
  }

  currentFileURL = URL.createObjectURL(file);
  runButton.disabled = false;
  transcriptBox.value = "";
  summaryBox.value = "";
  setStatus(`Selected: ${file.name}`);
});

runButton.addEventListener("click", async () => {
  if (!currentFileURL || !transcriber || !summarizer) return;

  runButton.disabled = true;
  transcriptBox.value = "";
  summaryBox.value = "";

  try {
    setStatus("Transcribing audio... this can take a while on CPU.");
    const transcriptionResult = await transcriber(currentFileURL, {
      chunk_length_s: 30,
      stride_length_s: 5,
    });

    const transcript = Array.isArray(transcriptionResult)
      ? transcriptionResult.map((r) => r.text).join(" ")
      : transcriptionResult.text;

    transcriptBox.value = transcript.trim();

    setStatus("Summarizing key points and decisions...");
    const prompt = buildSummaryPrompt(transcript);

    const output = await summarizer(
      [{ role: "user", content: prompt }],
      { max_new_tokens: 400, temperature: 0.3, do_sample: false }
    );

    const generated = output[0].generated_text;
    const summary = Array.isArray(generated)
      ? generated[generated.length - 1].content.trim()
      : String(generated).trim();

    summaryBox.value = summary;
    setStatus("Done.");
  } catch (err) {
    console.error(err);
    setStatus("Something went wrong. Check the console for details.");
  } finally {
    runButton.disabled = false;
  }
});

function buildSummaryPrompt(transcript) {
  return `The following is a transcript of a business meeting. List the key points discussed and any decisions that were made, as a concise bullet-point summary. If the transcript is unclear in places, do your best rather than guessing wildly.

Transcript:
${transcript}

Key points and decisions:`;
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
