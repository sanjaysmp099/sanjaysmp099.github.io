// ---- CONFIG ----
// Replace this with your actual Hugging Face Space API URL once deployed.
// Example: "https://your-username-pm-chatbot-backend.hf.space/chat"
const BACKEND_URL = "https://YOUR-USERNAME-pm-chatbot-backend.hf.space/chat";

const chatLog = document.getElementById("chat-log");
const chatForm = document.getElementById("chat-form");
const chatInput = document.getElementById("chat-input");
const loadingIndicator = document.getElementById("loading-indicator");

chatForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const question = chatInput.value.trim();
  if (!question) return;

  appendMessage("user", question);
  chatInput.value = "";
  setLoading(true);

  try {
    const response = await fetch(BACKEND_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ query: question }),
    });

    if (!response.ok) {
      throw new Error(`Backend returned status ${response.status}`);
    }

    const data = await response.json();
    appendMessage("bot", data.answer, data.sources);
  } catch (err) {
    appendMessage(
      "bot",
      "Sorry, I couldn't reach the assistant backend. It may be waking up from sleep — please try again in a few seconds."
    );
    console.error(err);
  } finally {
    setLoading(false);
  }
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
        .map(
          (s) =>
            `<a href="${s.url}" target="_blank" rel="noopener">${s.title}</a>`
        )
        .join(", ");
    el.appendChild(sourcesEl);
  }

  chatLog.appendChild(el);
  chatLog.scrollTop = chatLog.scrollHeight;
}

function setLoading(isLoading) {
  loadingIndicator.classList.toggle("hidden", !isLoading);
}
