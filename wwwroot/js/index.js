document.addEventListener("DOMContentLoaded", () => {
  const modalElement = document.getElementById("applicationModal");

  if (!modalElement) {
    return;
  }

  const modal = new bootstrap.Modal(modalElement);

  const form = document.getElementById("applicationForm");
  const modalTitle = document.getElementById("applicationModalLabel");
  const modalSubtitle = document.getElementById("applicationModalSubtitle");
  const saveButton = document.getElementById("applicationSaveButton");
  const actionsSection = document.getElementById("applicationActionsSection");

  const fields = {
    id: document.getElementById("applicationId"),
    company: document.getElementById("applicationCompany"),
    role: document.getElementById("applicationRole"),
    url: document.getElementById("applicationUrl"),
    status: document.getElementById("applicationStatus"),
    method: document.getElementById("applicationMethod"),
    dateApplied: document.getElementById("applicationDateApplied"),
    followUpDate: document.getElementById("applicationFollowUpDate"),
    contact: document.getElementById("applicationContact"),
    jobDescription: document.getElementById("applicationJobDescription"),
    notes: document.getElementById("applicationNotes"),
    resumePath: document.getElementById("applicationResumePath"),
    coverLetterPath: document.getElementById("applicationCoverLetterPath"),
  };

  /*
   * Pipeline filtering
   */
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

  /*
   * New Application
   */
  const newApplicationButton = document.getElementById("newApplicationButton");

  newApplicationButton?.addEventListener("click", () => {
    openNewApplication();
  });

  /*
   * Details buttons
   */
  document.querySelectorAll(".application-details").forEach((button) => {
    button.addEventListener("click", () => {
      openDetails(button);
    });
  });

  /*
   * Edit buttons
   */
  document.querySelectorAll(".application-edit").forEach((button) => {
    button.addEventListener("click", () => {
      openEdit(button);
    });
  });

  function openNewApplication() {
    clearForm();

    setReadOnly(false);

    modalTitle.textContent = "New Application";
    modalSubtitle.textContent = "Create a new application to track.";

    saveButton.textContent = "Create Application";

    form.action = "?handler=Create";

    actionsSection.classList.add("d-none");

    modal.show();
  }

  function openDetails(button) {
    populateForm(button);

    setReadOnly(true);

    modalTitle.textContent = "Application Details";
    modalSubtitle.textContent =
      "View application information and available actions.";

    saveButton.classList.add("d-none");

    actionsSection.classList.remove("d-none");

    modal.show();
  }

  function openEdit(button) {
    populateForm(button);

    setReadOnly(false);

    modalTitle.textContent = "Edit Application";
    modalSubtitle.textContent = "Update the application information.";

    saveButton.textContent = "Save Changes";

    saveButton.classList.remove("d-none");

    form.action = "?handler=Update";

    actionsSection.classList.add("d-none");

    modal.show();
  }

  function populateForm(button) {
    fields.id.value = button.dataset.id || "";
    fields.company.value = button.dataset.company || "";
    fields.role.value = button.dataset.role || "";
    fields.url.value = button.dataset.url || "";
    fields.status.value = button.dataset.status || "Bookmarked";
    fields.method.value = button.dataset.method || "";
    fields.dateApplied.value = button.dataset.dateApplied || "";
    fields.followUpDate.value = button.dataset.followUpDate || "";
    fields.contact.value = button.dataset.contact || "";
    fields.jobDescription.value = button.dataset.jobDescription || "";
    fields.notes.value = button.dataset.notes || "";
    fields.resumePath.value = button.dataset.resumePath || "";
    fields.coverLetterPath.value = button.dataset.coverLetterPath || "";
  }

  function clearForm() {
    form.reset();

    fields.id.value = "";
    fields.status.value = "Bookmarked";

    saveButton.classList.remove("d-none");
  }

  function setReadOnly(readOnly) {
    const editableFields = [
      fields.company,
      fields.role,
      fields.url,
      fields.status,
      fields.method,
      fields.dateApplied,
      fields.followUpDate,
      fields.contact,
      fields.jobDescription,
      fields.notes,
      fields.resumePath,
      fields.coverLetterPath,
    ];

    editableFields.forEach((field) => {
      field.readOnly = readOnly;

      if (field.tagName === "SELECT") {
        field.disabled = readOnly;
      }
    });
  }
});
