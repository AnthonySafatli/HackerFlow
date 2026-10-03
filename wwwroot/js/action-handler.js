window.hfHandlers = {
  copyPrompt: async ({ ok, data }) => {
    if (!ok) throw new Error("Request failed");
    await navigator.clipboard.writeText(data.prompt);
  },
};
