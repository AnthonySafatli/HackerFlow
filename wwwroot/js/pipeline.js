document.addEventListener("DOMContentLoaded", () => {
  const pipelineSteps = document.querySelectorAll(".pipeline-step");
  const applicationRows = document.querySelectorAll(
    "#applicationsTable tbody tr",
  );

  function applyFilter(status) {
    applicationRows.forEach((row) => {
      row.style.display =
        status === "all" || row.dataset.status === status ? "" : "none";
    });
  }

  pipelineSteps.forEach((step) => {
    step.addEventListener("click", (event) => {
      event.preventDefault();

      pipelineSteps.forEach((item) => item.classList.remove("active"));
      step.classList.add("active");

      applyFilter(step.dataset.status);
    });
  });

  // Apply the initial filter
  const initial = document.querySelector(".pipeline-step.active");
  if (initial) applyFilter(initial.dataset.status);
});
