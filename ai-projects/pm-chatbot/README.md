# PM Chatbot Frontend

Static chat UI, deployed via GitHub Pages.

## Deploying

1. Copy this `frontend/` folder's contents into
   `ai-projects/pm-chatbot/` in your `sanjaysmp099.github.io` repo.
2. Edit `script.js` and set `BACKEND_URL` to your deployed Hugging Face
   Space's `/chat` endpoint (see `backend/README.md` for how to get this).
3. Commit and push. GitHub Pages will serve it automatically at:
   `https://sanjaysmp099.github.io/ai-projects/pm-chatbot/`

No build step, no dependencies — plain HTML/CSS/JS.
