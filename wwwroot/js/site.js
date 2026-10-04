const navStateKey = "toolLogixNavOpen";
const themeStorageKey = "toolLogixTheme";

function getPreferredTheme() {
    const savedTheme = localStorage.getItem(themeStorageKey);

    if (savedTheme === "dark" || savedTheme === "light") {
        return savedTheme;
    }

    return window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "dark"
        : "light";
}

function applyTheme(theme) {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme;
}

// Apply the saved or system theme as soon as site.js loads.
applyTheme(getPreferredTheme());

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

    // Treat these routes as the ToolLogix homepage.
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

function initializeThemeToggle() {
    const toggle = document.getElementById("darkModeToggle");
    const icon = document.getElementById("themeIcon");
    const currentTheme = getPreferredTheme();

    applyTheme(currentTheme);

    // The toggle only exists on the Profile page.
    if (!toggle) {
        return;
    }

    toggle.checked = currentTheme === "dark";

    if (icon) {
        icon.textContent = toggle.checked ? "🌙" : "☀️";
    }

    toggle.addEventListener("change", function () {
        const theme = toggle.checked ? "dark" : "light";

        applyTheme(theme);
        localStorage.setItem(themeStorageKey, theme);

        if (icon) {
            icon.textContent = theme === "dark" ? "🌙" : "☀️";
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const storedState = sessionStorage.getItem(navStateKey);

    // Open the navigation by default if no state has been saved.
    const isOpen =
        storedState === null
            ? true
            : storedState === "true";

    setNavState(isOpen);
    highlightCurrentPage();
    initializeThemeToggle();
});

document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
        setNavState(false);
    }
});