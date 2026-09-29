const navStateKey = "toolLogixNavOpen";

function getNavElements() {
    return {
        sidenav: document.getElementById("toolLogixSidenav"),
        pageContent: document.getElementById("pageContent"),
        toggleButton: document.getElementById("navToggleButton")
    };
}

function setNavState(isOpen) {
    const { sidenav, pageContent, toggleButton } = getNavElements();

    if (!sidenav || !pageContent || !toggleButton) {
        return;
    }

    sidenav.classList.toggle("sidenav--open", isOpen);
    pageContent.classList.toggle("page-content--shifted", isOpen);
    toggleButton.classList.toggle("nav-toggle--open", isOpen);

    sidenav.setAttribute("aria-hidden", String(!isOpen));
    toggleButton.setAttribute("aria-expanded", String(isOpen));
    toggleButton.setAttribute(
        "aria-label",
        isOpen ? "Close navigation" : "Open navigation"
    );

    sessionStorage.setItem(navStateKey, String(isOpen));
}

function toggleNav() {
    const { sidenav } = getNavElements();

    if (!sidenav) {
        return;
    }

    const isOpen = !sidenav.classList.contains("sidenav--open");
    setNavState(isOpen);
}

function normalizePath(path) {
    let normalizedPath = path.toLowerCase();

    if (normalizedPath.length > 1) {
        normalizedPath = normalizedPath.replace(/\/+$/, "");
    }

    // Treat all of these as the ToolLogix homepage.
    if (
        normalizedPath === "/home" ||
        normalizedPath === "/home/index"
    ) {
        return "/";
    }

    return normalizedPath || "/";
}

function highlightCurrentPage() {
    const currentPath = normalizePath(window.location.pathname);
    const navigationLinks =
        document.querySelectorAll(".sidenav__link");

    navigationLinks.forEach(link => {
        const linkUrl = new URL(link.href, window.location.origin);
        const linkPath = normalizePath(linkUrl.pathname);
        const isCurrentPage = linkPath === currentPath;

        link.classList.toggle("active", isCurrentPage);

        if (isCurrentPage) {
            link.setAttribute("aria-current", "page");
        } else {
            link.removeAttribute("aria-current");
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const storedState = sessionStorage.getItem(navStateKey);

    // Open by default when no preference has been saved.
    const isOpen = storedState === null
        ? true
        : storedState === "true";

    setNavState(isOpen);
    highlightCurrentPage();
});

document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
        setNavState(false);
    }
});