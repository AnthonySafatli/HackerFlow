window.hfHandlers = {
  copyPrompt: async ({ ok, data }) => {
    if (!ok) throw new Error("Request failed");
    await navigator.clipboard.writeText(data.prompt);
  },
  resumeGenerated: async ({ ok, data }) => {
    if (!ok) throw new Error("Request failed");
    document.getElementById("resume-message").textContent =
      "Resume generated successfully";
  },
  coverLetterGenerated: async ({ ok, data }) => {
    if (!ok) throw new Error("Request failed");
    document.getElementById("cover-letter-message").textContent =
      "Cover letter generated successfully";
  },
};
