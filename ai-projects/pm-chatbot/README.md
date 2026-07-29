# PM Chatbot (fully client-side)

No backend, no server, no Hugging Face Space. Everything — embeddings,
retrieval, the guardrail, and the LLM itself — runs in the visitor's
browser using WebGPU. This deploys as plain static files on GitHub Pages.

## How it works

- **Embeddings**: `@xenova/transformers` runs `bge-small-en-v1.5` in the
  browser to turn the user's question into a vector.
- **Retrieval**: the question's vector is compared (cosine similarity)
  against precomputed vectors for every knowledge base chunk, stored in
  `data/kb-embeddings.json`.
- **Guardrail**: if the best-matching chunk's similarity score is below a
  threshold, the question is treated as off-topic and politely declined —
  no LLM call needed for that case.
- **Generation**: `@mlc-ai/web-llm` runs `Llama-3.2-1B-Instruct` in the
  browser via WebGPU to generate the final answer from the retrieved
  context.
- **Citations**: each chunk carries its source title and URL, attached to
  every answer automatically.

## One-time setup: generate the embeddings file

`data/kb-embeddings.json` is not something you write by hand — it's
generated from the files in `knowledge_base/`. You need Node.js installed
(nodejs.org) to run this once before your first deploy, and again any time
you add or edit knowledge base files.

```bash
cd scripts
npm install
node build-embeddings.mjs
```

This downloads the embedding model once (cached locally by npm/node),
embeds every chunk in `knowledge_base/*.md`, and writes
`../data/kb-embeddings.json`. Commit that generated file — it's what
actually ships to your site's visitors.

## Deploying to GitHub Pages

1. Copy this entire `frontend/` folder's contents into
   `ai-projects/pm-chatbot/` in your `sanjaysmp099.github.io` repo
   (replacing the old files from the backend-based version).
2. Make sure `data/kb-embeddings.json` is committed (see step above).
3. Commit and push.
4. Visit `https://sanjaysmp099.github.io/ai-projects/pm-chatbot/`

That's it — no separate backend deployment step at all.

## What visitors experience

- First visit: the page downloads the embedding model (~30MB) and the LLM
  (~800MB for the 1B model) into the browser. This takes a bit, with a
  progress bar shown during load.
- The browser caches these afterward, so repeat visits are fast.
- Requires a WebGPU-capable browser (recent Chrome/Edge). Older browsers
  or those without WebGPU won't be able to run the LLM step.

## Adding more knowledge base content

Add a new `.md` file to `knowledge_base/` in this format:

```
---
title: Your document title
source_url: https://source-link.com
---

Body text in your own words, from a source you have the right to use.
```

Then re-run the build script and commit the updated
`data/kb-embeddings.json`.

## Tuning the guardrail

If the assistant seems too strict (rejecting real PM questions) or too
loose (answering off-topic questions), adjust `SIMILARITY_THRESHOLD` in
`script.js`. Lower = more permissive, higher = stricter.
