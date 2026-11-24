(() => {
    "use strict";

    const form = document.querySelector("[data-asset-search]");
    const input = document.getElementById("assetNumber");
    const resultsRegion =
        document.getElementById("asset-results-region");

    if (!form || !input || !resultsRegion) {
        return;
    }

    let debounceTimer;
    let activeRequest;

    const refreshResults = async () => {
        const searchTerm = input.value.trim();

        if (activeRequest) {
            activeRequest.abort();
        }

        activeRequest = new AbortController();

        const requestUrl = new URL(
            form.dataset.searchUrl,
            window.location.origin);

        if (searchTerm) {
            requestUrl.searchParams.set(
                "assetNumber",
                searchTerm);
        }

        resultsRegion.setAttribute("aria-busy", "true");

        try {
            const response = await fetch(requestUrl, {
                signal: activeRequest.signal,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error(
                    `Asset search failed: ${response.status}`);
            }

            resultsRegion.innerHTML =
                await response.text();

            const browserUrl = new URL(window.location.href);

            if (searchTerm) {
                browserUrl.searchParams.set(
                    "assetNumber",
                    searchTerm);
            } else {
                browserUrl.searchParams.delete("assetNumber");
            }

            window.history.replaceState(
                {},
                "",
                browserUrl);
        }
        catch (error) {
            if (error.name !== "AbortError") {
                console.error(error);
            }
        }
        finally {
            resultsRegion.removeAttribute("aria-busy");
        }
    };

    input.addEventListener("input", () => {
        window.clearTimeout(debounceTimer);

        debounceTimer = window.setTimeout(
            refreshResults,
            300);
    });

    form.addEventListener("submit", event => {
        event.preventDefault();
        window.clearTimeout(debounceTimer);
        refreshResults();
    });

    input.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            input.value = "";
            window.clearTimeout(debounceTimer);
            refreshResults();
        }
    });
})();