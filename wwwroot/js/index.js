document.addEventListener("DOMContentLoaded", () => {
  const pipelineSteps = document.querySelectorAll(".pipeline-step");
  const applicationRows = document.querySelectorAll(
    "#applicationsTable tbody tr",
  );

  const applicationModalElement = document.getElementById("applicationModal");
  const applicationModal = applicationModalElement
    ? new bootstrap.Modal(applicationModalElement)
    : null;

  const modalTitle = document.getElementById("applicationModalLabel");
  const newApplicationButton = document.getElementById("newApplicationButton");

  /*
   * Pipeline filtering
   */
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

        if (!status || status === "all" || rowStatus === status) {
          row.style.display = "";
        } else {
          row.style.display = "none";
        }
      });
    });
  });

  /*
   * New Application
   */
  newApplicationButton?.addEventListener("click", () => {
    openApplicationModal("new");
  });

  /*
   * Details / Open buttons
   */
  document.querySelectorAll(".application-details").forEach((button) => {
    button.addEventListener("click", () => {
      openApplicationModal("details", button);
    });
  });

  /*
   * Shared application modal
   */
  function openApplicationModal(mode, button = null) {
    if (!applicationModal) {
      return;
    }

    if (mode === "new") {
      modalTitle.textContent = "New Application";
    }

    if (mode === "details") {
      modalTitle.textContent = "Application Details";

      // Application data can be loaded here later.
      // For now, the modal remains blank.
      const applicationId = button?.dataset.applicationId;

      console.log("Opening application:", applicationId);
    }

    applicationModal.show();
  }
});
