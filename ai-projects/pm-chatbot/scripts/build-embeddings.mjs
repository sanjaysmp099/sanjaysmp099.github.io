/**
 * Run this once locally (not in the browser) whenever you add or change
 * files in knowledge_base/. It chunks each file, embeds every chunk with
 * the same model the browser will use for queries, and writes the result
 * to data/kb-embeddings.json, which is what actually ships to visitors.
 *
 * Usage:
 *   cd scripts
 *   npm install
 *   node build-embeddings.mjs
 */

import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";
import { pipeline } from "@xenova/transformers";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const KB_DIR = path.join(__dirname, "..", "knowledge_base");
const OUT_FILE = path.join(__dirname, "..", "data", "kb-embeddings.json");
const EMBED_MODEL_ID = "Xenova/bge-small-en-v1.5";

const CHUNK_SIZE = 800;
const CHUNK_OVERLAP = 150;

function parseFile(content) {
  const match = content.match(/^---\s*([\s\S]*?)\s*---\s*([\s\S]*)$/);
  if (!match) {
    throw new Error("File is missing the --- metadata header ---");
  }
  const [, header, body] = match;
  const meta = {};
  header.trim().split("\n").forEach((line) => {
    const idx = line.indexOf(":");
    if (idx === -1) return;
    meta[line.slice(0, idx).trim()] = line.slice(idx + 1).trim();
  });
  return { title: meta.title, source_url: meta.source_url, body: body.trim() };
}

function chunkText(text, size = CHUNK_SIZE, overlap = CHUNK_OVERLAP) {
  const chunks = [];
  let start = 0;
  while (start < text.length) {
    chunks.push(text.slice(start, start + size).trim());
    start += size - overlap;
  }
  return chunks.filter(Boolean);
}

async function main() {
  console.log("Loading embedding model (downloads once, cached after)...");
  const embedder = await pipeline("feature-extraction", EMBED_MODEL_ID);

  const files = fs.readdirSync(KB_DIR).filter((f) => f.endsWith(".md"));
  console.log(`Found ${files.length} knowledge base files.`);

  const output = [];

  for (const file of files) {
    const content = fs.readFileSync(path.join(KB_DIR, file), "utf-8");
    const { title, source_url, body } = parseFile(content);
    const chunks = chunkText(body);
    console.log(`  ${file}: ${chunks.length} chunks`);

    for (const chunk of chunks) {
      const result = await embedder(chunk, { pooling: "mean", normalize: true });
      output.push({
        text: chunk,
        title,
        source_url,
        embedding: Array.from(result.data),
      });
    }
  }

  fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
  fs.writeFileSync(OUT_FILE, JSON.stringify(output));
  console.log(`\nWrote ${output.length} chunks to ${OUT_FILE}`);
  console.log("Commit this file along with your knowledge_base/ changes.");
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
