document.addEventListener("DOMContentLoaded", () => {
  const pipelineSteps = document.querySelectorAll(".pipeline-step");
  const applicationRows = document.querySelectorAll(
    "#applicationsTable tbody tr",
  );

  const validStatuses = Array.from(pipelineSteps).map((s) => s.dataset.status);
  const defaultStatus = "bookmarked";

  function applyFilter(status) {
    applicationRows.forEach((row) => {
      row.style.display =
        status === "all" || row.dataset.status === status ? "" : "none";
    });
  }

  function setActive(status) {
    pipelineSteps.forEach((item) =>
      item.classList.toggle("active", item.dataset.status === status),
    );
    applyFilter(status);
  }

  function statusFromUrl() {
    const hash = window.location.hash.slice(1).toLowerCase();
    return validStatuses.includes(hash) ? hash : defaultStatus;
  }

  pipelineSteps.forEach((step) => {
    step.addEventListener("click", (event) => {
      event.preventDefault();
      const status = step.dataset.status;

      history.pushState(null, "", `#${status}`);
      setActive(status);
    });
  });

  window.addEventListener("popstate", () => setActive(statusFromUrl()));

  setActive(statusFromUrl());
});

document.addEventListener("submit", (e) => {
  const form = e.target;
  if (form.method.toLowerCase() !== "post" || !location.hash) return;
  form.action = form.action.split("#")[0] + location.hash;
});
