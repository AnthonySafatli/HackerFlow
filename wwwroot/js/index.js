document.addEventListener("DOMContentLoaded", () => {
  const pipelineSteps = document.querySelectorAll(".pipeline-step");
  const applicationRows = document.querySelectorAll(
    "#applicationsTable tbody tr",
  );

  pipelineSteps.forEach((step) => {
    step.addEventListener("click", (event) => {
      event.preventDefault();

      const status = step.dataset.status;

      pipelineSteps.forEach((item) => {
        item.classList.remove("active");
      });

      step.classList.add("active");

      applicationRows.forEach((row) => {
        const rowStatus = row.dataset.status;

        row.style.display =
          status === "all" || rowStatus === status ? "" : "none";
      });
    });
  });
});
