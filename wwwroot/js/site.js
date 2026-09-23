function openNav() {
    const sidenav = document.getElementById("toolLogixSidenav");
    const pageContent = document.getElementById("pageContent");
    const openButton = document.getElementById("openNavButton");

    sidenav.classList.add("sidenav--open");
    pageContent.classList.add("page-content--shifted");

    sidenav.setAttribute("aria-hidden", "false");
    openButton.setAttribute("aria-expanded", "true");
}

function closeNav() {
    const sidenav = document.getElementById("toolLogixSidenav");
    const pageContent = document.getElementById("pageContent");
    const openButton = document.getElementById("openNavButton");

    sidenav.classList.remove("sidenav--open");
    pageContent.classList.remove("page-content--shifted");

    sidenav.setAttribute("aria-hidden", "true");
    openButton.setAttribute("aria-expanded", "false");
}