(() => {
    const input = document.getElementById("query");
    const list = document.getElementById("search-suggestions");
    if (!input || !list) return;

    let timer;
    let controller;
    const close = () => {
        list.hidden = true;
        list.replaceChildren();
        input.setAttribute("aria-expanded", "false");
    };

    input.addEventListener("input", () => {
        window.clearTimeout(timer);
        if (controller) controller.abort();
        const query = input.value.trim();
        if (query.length < 2) {
            close();
            return;
        }

        timer = window.setTimeout(async () => {
            controller = new AbortController();
            try {
                const response = await fetch(`/api/catalog/suggestions?query=${encodeURIComponent(query)}`, { signal: controller.signal });
                if (!response.ok) throw new Error("Suggestions request failed.");
                const suggestions = await response.json();
                list.replaceChildren();
                suggestions.forEach((suggestion) => {
                    const option = document.createElement("button");
                    option.type = "button";
                    option.className = "suggestion-option";
                    option.setAttribute("role", "option");
                    const text = document.createElement("span");
                    text.textContent = suggestion.text;
                    const type = document.createElement("small");
                    type.textContent = suggestion.type;
                    option.append(text, type);
                    option.addEventListener("click", () => {
                        input.value = suggestion.text;
                        close();
                        input.form?.requestSubmit();
                    });
                    list.append(option);
                });
                list.hidden = suggestions.length === 0;
                input.setAttribute("aria-expanded", String(!list.hidden));
            } catch (error) {
                if (error.name !== "AbortError") close();
            }
        }, 180);
    });

    document.addEventListener("click", (event) => {
        if (!list.contains(event.target) && event.target !== input) close();
    });
    input.addEventListener("keydown", (event) => {
        if (event.key === "Escape") close();
    });
})();
