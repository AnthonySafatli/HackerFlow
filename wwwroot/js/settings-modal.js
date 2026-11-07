(() => {
  const modalEl = document.getElementById("settingsModal");
  const form = document.getElementById("settingsForm");
  if (!modalEl || !form) return;

  const badge = document.getElementById("settingsUnsaved");
  const saveBtn = document.getElementById("settingsSave");
  let saving = false;

  const refresh = () => {
    let anyDirty = false;

    form.querySelectorAll(".hf-field").forEach((field) => {
      const dirty = [...field.querySelectorAll("input, textarea, select")].some(
        (el) => el.value !== el.defaultValue,
      );
      field.classList.toggle("is-dirty", dirty);
      anyDirty ||= dirty;
    });

    badge.hidden = !anyDirty;
    saveBtn.disabled = !anyDirty;
    return anyDirty;
  };

  form.addEventListener("input", refresh);
  form.addEventListener("submit", () => {
    saving = true;
  });

  modalEl.addEventListener("hide.bs.modal", (e) => {
    if (saving || !refresh()) return;

    if (!confirm("You have unsaved changes. Close without saving?")) {
      e.preventDefault();
      return;
    }
    form.reset();
    refresh();
  });
})();
