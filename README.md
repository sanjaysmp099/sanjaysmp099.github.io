# Portfolio site

A single-page portfolio site framed like a project status board — fitting, since
you're an AI Project Manager. Career history renders as a Gantt-style timeline,
your featured build shows up as a "shipped" project card, all in one `index.html`
with no build step and no dependencies.

## 1. Customize your content

Open `index.html`, scroll to the `<script>` tag near the bottom, and edit the
`CONFIG` object. Everything you need to change is there:
- name, role, tagline, status
- about text
- career timeline entries
- your featured project (title, description, links)
- skills
- contact links

You don't need to touch any HTML or CSS above it.

**Add your résumé:** drop a file named `resume.pdf` in this same folder — the
"View résumé" button already points to it.

## 2. Deploy free on GitHub Pages

1. Create a new **public** GitHub repo (e.g. `your-username.github.io` if you want
   it at the root of your GitHub domain, or any repo name if you're fine with a
   `/repo-name/` path).
2. Push this folder's contents to that repo:
   ```bash
   git init
   git add .
   git commit -m "Initial portfolio site"
   git branch -M main
   git remote add origin https://github.com/your-username/your-repo.git
   git push -u origin main
   ```
3. In the repo, go to **Settings → Pages**.
4. Under "Source," choose the `main` branch and `/ (root)` folder, then Save.
5. GitHub gives you a live URL within a minute or two:
   - `https://your-username.github.io/` (if repo is named `your-username.github.io`)
   - `https://your-username.github.io/your-repo/` (otherwise)

That URL is what goes on your resume/LinkedIn.

## 3. Connect it to AI Project Pulse

Once you've deployed the AI Project Pulse app (Streamlit Community Cloud or
Hugging Face Spaces), copy its live URL and paste it into `CONFIG.project.demoUrl`
in `index.html`. Also update `githubUrl` to point at that project's repo, and
`writeupUrl` to point at the case study section of its README (or a dedicated page).

Commit and push the change — GitHub Pages redeploys automatically within a minute.

## 4. Optional: custom domain

If you own a domain, add a `CNAME` file to this repo containing just your domain
(e.g. `yourname.com`), and point your domain's DNS to GitHub Pages per
[GitHub's custom domain docs](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site).
Not required — the free `github.io` URL works fine for a resume link.
