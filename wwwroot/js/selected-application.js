document.addEventListener("DOMContentLoaded", () => {
  const prefix = "detailsApplicationModal-";
  const modals = document.querySelectorAll(`[id^="${prefix}"]`);

  function setSelected(id) {
    const url = new URL(window.location.href);
    if (id) url.searchParams.set("selected", id);
    else url.searchParams.delete("selected");
    history.replaceState(null, "", url); // keeps the #status hash too
  }

  modals.forEach((modal) => {
    const id = modal.id.slice(prefix.length);
    modal.addEventListener("shown.bs.modal", () => setSelected(id));
    modal.addEventListener("hidden.bs.modal", () => setSelected(null));
  });

  // On load, open the modal named in the URL
  const selected = new URLSearchParams(window.location.search).get("selected");
  if (selected) {
    const el = document.getElementById(prefix + selected);
    if (el) bootstrap.Modal.getOrCreateInstance(el).show();
  }
});
