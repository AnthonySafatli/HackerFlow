// Include once (e.g. in _Layout), together with action-handlers.js.
async function hfRunAction(btn, url, method, handlerName, value) {
  if (btn.dataset.busy) return;

  const label = btn.querySelector(".hf-action-label");
  const spinner = btn.querySelector(".hf-action-spinner");
  const okIcon = btn.querySelector(".hf-action-ok");
  const failIcon = btn.querySelector(".hf-action-fail");
  const token = document.querySelector(
    'input[name="__RequestVerificationToken"]',
  )?.value;

  btn.dataset.busy = "1";
  btn.disabled = true;
  label.classList.add("d-none");
  spinner.classList.remove("d-none");

  let ok = false,
    data = null;
  try {
    const res = await fetch(url, {
      method: method || "POST",
      headers: token ? { RequestVerificationToken: token } : {},
    });
    ok = res.ok;
    if (ok && (res.headers.get("content-type") || "").includes("json"))
      data = await res.json();
  } catch {
    /* network error -> ok stays false */
  }

  // Run the page's handler; if it throws, the button shows ✕
  const handler = handlerName && window.hfHandlers?.[handlerName];
  if (handler) {
    try {
      await handler({ ok, data, value, btn });
    } catch (err) {
      console.error(err);
      ok = false;
    }
  }

  spinner.classList.add("d-none");
  (ok ? okIcon : failIcon).classList.remove("d-none");

  setTimeout(() => {
    okIcon.classList.add("d-none");
    failIcon.classList.add("d-none");
    label.classList.remove("d-none");
    btn.disabled = false;
    delete btn.dataset.busy;
  }, 1500);
}

document.addEventListener("click", (e) => {
  const btn = e.target.closest(".hf-action-btn");
  if (btn)
    return hfRunAction(
      btn,
      btn.dataset.actionUrl,
      btn.dataset.actionMethod,
      btn.dataset.handler,
    );

  const item = e.target.closest(".hf-dropdown-item");
  if (item) {
    e.preventDefault();
    const toggle = item.closest(".dropdown").querySelector(".hf-dropdown-btn");
    const url = new URL(item.dataset.actionUrl, location.origin);
    url.searchParams.set(item.dataset.paramName, item.dataset.value);
    return hfRunAction(
      toggle,
      url,
      item.dataset.actionMethod,
      item.dataset.handler,
      item.dataset.value,
    );
  }
});
