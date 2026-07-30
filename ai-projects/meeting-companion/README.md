# Meeting Companion (fully client-side)

Uploads a meeting recording, transcribes it with Whisper, and summarizes key
points and decisions with a small open-source LLM — entirely in the
browser. No backend, no server, no Gradio (Gradio is Python-only and can't
run on GitHub Pages or any current free hosting tier).

## How it works

- **Transcription**: `@huggingface/transformers` runs `Xenova/whisper-tiny.en`
  in the browser to convert the uploaded audio into text.
- **Summarization**: the same library runs `onnx-community/gemma-3-270m-it-ONNX`
  to extract key points and decisions from the transcript.
- Both models run on CPU via WebAssembly (not WebGPU), so this works on any
  hardware, including older machines without a capable GPU — same lesson
  learned from the PM chatbot project.

## Deploying to GitHub Pages

1. Copy this folder's contents into a new folder in your
   `sanjaysmp099.github.io` repo, e.g. `ai-projects/meeting-companion/`.
2. Commit and push.
3. Visit `https://sanjaysmp099.github.io/ai-projects/meeting-companion/`

No build step, no separate backend to deploy — this is the entire app.

## What to expect

- First visit downloads both models into the browser (a few hundred MB
  total) — a progress bar shows this.
- Transcription and summarization both run on CPU, so processing a several-
  minute recording can take a while, especially on older hardware. This is
  the same trade-off as the PM chatbot: slower, but works everywhere with
  zero cost and no server dependency.
- Works best with clear, English-language audio (the model used here is
  the English-only Whisper variant, matching the original lab).

## Adjusting quality vs. speed

- Swap `Xenova/whisper-tiny.en` for `Xenova/whisper-base.en` for better
  accuracy at the cost of a larger download and slower processing.
- Swap `onnx-community/gemma-3-270m-it-ONNX` for a larger instruct model if
  you want richer summaries and don't mind longer load/generation times.
