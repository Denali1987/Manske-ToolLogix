(() => {
    "use strict";

    const dataElement = document.getElementById("downtime-analytics-data");
    const chartElement = document.getElementById("downtime-bar-chart");
    const tableBody = document.getElementById("downtime-analytics-table-body");

    if (!dataElement || !chartElement || !tableBody) {
        return;
    }

    const records = JSON.parse(dataElement.textContent || "[]");
    const groupBy = document.getElementById("analytics-group-by");
    const topCount = document.getElementById("analytics-top-count");
    const workCenters = document.getElementById("analytics-work-centers");
    const tools = document.getElementById("analytics-tools");
    const tableSearch = document.getElementById("analytics-table-search");
    const recordCount = document.getElementById("analytics-record-count");
    const totalHours = document.getElementById("analytics-total-hours");
    const selectedCodeCount = document.getElementById("analytics-selected-code-count");
    const codeCheckboxes = Array.from(document.querySelectorAll("[data-analytics-code]"));
    const columnFilters = Array.from(document.querySelectorAll("[data-column-filter]"));
    const sortButtons = Array.from(document.querySelectorAll("[data-sort-key]"));

    let sortKey = "durationMinutes";
    let sortDirection = "desc";

    const selectedValues = (select) =>
        new Set(Array.from(select.selectedOptions).map(option => option.value));

    const selectedCodes = () =>
        new Set(codeCheckboxes.filter(input => input.checked).map(input => input.value));

    const baseFilteredRecords = () => {
        const codes = selectedCodes();
        const selectedWorkCenters = selectedValues(workCenters);
        const selectedTools = selectedValues(tools);

        return records.filter(record => {
            if (!codes.has(record.downtimeReasonCode)) {
                return false;
            }

            if (selectedWorkCenters.size > 0 &&
                !selectedWorkCenters.has(record.workCenter)) {
                return false;
            }

            if (selectedTools.size > 0 &&
                !selectedTools.has(record.tool || "")) {
                return false;
            }

            return true;
        });
    };

    const formatDate = (value) => {
        if (!value) {
            return "—";
        }

        return new Date(value).toLocaleString([], {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "numeric",
            minute: "2-digit"
        });
    };

    const formatHours = minutes => (Number(minutes || 0) / 60).toFixed(2);

    const updateSummary = filtered => {
        const hours = filtered.reduce(
            (sum, record) => sum + Number(record.durationMinutes || 0),
            0) / 60;

        recordCount.textContent = `${filtered.length.toLocaleString()} record${filtered.length === 1 ? "" : "s"}`;
        totalHours.textContent = `${hours.toFixed(2)} hours`;

        const count = selectedCodes().size;
        selectedCodeCount.textContent = `${count} selected`;
    };

    const renderChart = filtered => {
        const key = groupBy.value;
        const limit = Number(topCount.value);
        const totals = new Map();

        filtered.forEach(record => {
            const label = String(record[key] || "").trim() ||
                (key === "tool" ? "(No tool listed)" : "(Blank)");
            totals.set(label, (totals.get(label) || 0) + Number(record.durationMinutes || 0));
        });

        const ranked = Array.from(totals, ([label, minutes]) => ({ label, minutes }))
            .sort((a, b) => b.minutes - a.minutes)
            .slice(0, limit);

        chartElement.replaceChildren();

        if (ranked.length === 0) {
            const empty = document.createElement("p");
            empty.className = "downtime-bar-chart__empty";
            empty.textContent = "No records match the selected filters.";
            chartElement.appendChild(empty);
            return;
        }

        const maxMinutes = ranked[0].minutes;

        ranked.forEach((item, index) => {
            const row = document.createElement("div");
            row.className = "downtime-bar-chart__row";

            const rank = document.createElement("span");
            rank.className = "downtime-bar-chart__rank";
            rank.textContent = String(index + 1);

            const label = document.createElement("span");
            label.className = "downtime-bar-chart__label";
            label.textContent = item.label;
            label.title = item.label;

            const track = document.createElement("div");
            track.className = "downtime-bar-chart__track";

            const fill = document.createElement("div");
            fill.className = "downtime-bar-chart__fill";
            fill.style.width = `${Math.max(1.5, item.minutes / maxMinutes * 100)}%`;
            track.appendChild(fill);

            const value = document.createElement("strong");
            value.className = "downtime-bar-chart__value";
            value.textContent = `${formatHours(item.minutes)} h`;

            row.append(rank, label, track, value);
            chartElement.appendChild(row);
        });
    };

    const valueForFilter = (record, key) => {
        if (key === "planned" || key === "costed") {
            return String(Boolean(record[key]));
        }

        return String(record[key] ?? "").toLowerCase();
    };

    const applyTableFilters = filtered => {
        const search = (tableSearch.value || "").trim().toLowerCase();

        return filtered.filter(record => {
            for (const filter of columnFilters) {
                const filterValue = (filter.value || "").trim().toLowerCase();
                if (!filterValue) {
                    continue;
                }

                const key = filter.dataset.columnFilter;
                const value = valueForFilter(record, key);
                const isBoolean = key === "planned" || key === "costed";

                if (isBoolean ? value !== filterValue : !value.includes(filterValue)) {
                    return false;
                }
            }

            if (!search) {
                return true;
            }

            return [
                record.workCenter,
                record.tool,
                record.downtimeReasonCode,
                record.downtimeReason,
                record.employeeName,
                record.workOrder,
                record.partNumber
            ].some(value => String(value ?? "").toLowerCase().includes(search));
        });
    };

    const sortRecords = rows => {
        const multiplier = sortDirection === "asc" ? 1 : -1;

        return [...rows].sort((a, b) => {
            let left = a[sortKey];
            let right = b[sortKey];

            if (sortKey === "durationMinutes") {
                left = Number(left || 0);
                right = Number(right || 0);
            } else if (sortKey === "startTime" || sortKey === "endTime") {
                left = new Date(left).getTime();
                right = new Date(right).getTime();
            } else {
                left = String(left ?? "").toLowerCase();
                right = String(right ?? "").toLowerCase();
            }

            if (left < right) return -1 * multiplier;
            if (left > right) return 1 * multiplier;
            return 0;
        });
    };

    const appendCell = (row, text, className = "") => {
        const cell = document.createElement("td");
        cell.textContent = text;
        if (className) {
            cell.className = className;
        }
        row.appendChild(cell);
    };

    const renderTable = filtered => {
        const rows = sortRecords(applyTableFilters(filtered));
        tableBody.replaceChildren();

        sortButtons.forEach(button => {
            button.dataset.direction = button.dataset.sortKey === sortKey
                ? sortDirection
                : "";
        });

        if (rows.length === 0) {
            const row = document.createElement("tr");
            const cell = document.createElement("td");
            cell.colSpan = 12;
            cell.className = "downtime-table__empty";
            cell.textContent = "No downtime records match the selected table filters.";
            row.appendChild(cell);
            tableBody.appendChild(row);
            return;
        }

        rows.forEach(record => {
            const row = document.createElement("tr");
            appendCell(row, record.workCenter || "—");
            appendCell(row, record.tool || "—");
            appendCell(row, record.downtimeReasonCode || "—");
            appendCell(row, record.downtimeReason || "—");
            appendCell(row, record.employeeName || "—");
            appendCell(row, record.workOrder || "—");
            appendCell(row, record.partNumber || "—");
            appendCell(row, formatHours(record.durationMinutes), "downtime-table__duration");
            appendCell(row, formatDate(record.startTime), "downtime-table__date");
            appendCell(row, formatDate(record.endTime), "downtime-table__date");
            appendCell(row, record.planned ? "Yes" : "No");
            appendCell(row, record.costed ? "Yes" : "No");
            tableBody.appendChild(row);
        });
    };

    const updateAll = () => {
        const filtered = baseFilteredRecords();
        updateSummary(filtered);
        renderChart(filtered);
        renderTable(filtered);
    };

    [groupBy, topCount, workCenters, tools].forEach(control =>
        control.addEventListener("change", updateAll));

    codeCheckboxes.forEach(input => input.addEventListener("change", updateAll));
    tableSearch.addEventListener("input", () => renderTable(baseFilteredRecords()));

    columnFilters.forEach(filter => {
        filter.addEventListener("input", () => renderTable(baseFilteredRecords()));
        filter.addEventListener("change", () => renderTable(baseFilteredRecords()));
    });

    sortButtons.forEach(button => button.addEventListener("click", () => {
        const nextKey = button.dataset.sortKey;
        sortDirection = sortKey === nextKey && sortDirection === "asc" ? "desc" : "asc";
        sortKey = nextKey;
        renderTable(baseFilteredRecords());
    }));

    document.getElementById("analytics-select-active-codes").addEventListener("click", () => {
        codeCheckboxes.forEach(input => {
            input.checked = input.dataset.defaultSelected === "true";
        });
        updateAll();
    });

    document.getElementById("analytics-select-all-codes").addEventListener("click", () => {
        codeCheckboxes.forEach(input => input.checked = true);
        updateAll();
    });

    document.getElementById("analytics-clear-codes").addEventListener("click", () => {
        codeCheckboxes.forEach(input => input.checked = false);
        updateAll();
    });

    document.getElementById("analytics-reset-filters").addEventListener("click", () => {
        groupBy.value = "tool";
        topCount.value = "15";
        Array.from(workCenters.options).forEach(option => option.selected = false);
        Array.from(tools.options).forEach(option => option.selected = false);
        codeCheckboxes.forEach(input => {
            input.checked = input.dataset.defaultSelected === "true";
        });
        columnFilters.forEach(filter => filter.value = "");
        tableSearch.value = "";
        sortKey = "durationMinutes";
        sortDirection = "desc";
        updateAll();
    });

    updateAll();
})();
