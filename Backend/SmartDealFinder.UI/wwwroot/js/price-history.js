(() => {
    const container = document.getElementById("price-history-chart");
    if (!container) return;

    const showMessage = (message) => {
        const paragraph = document.createElement("p");
        paragraph.className = "muted";
        paragraph.textContent = message;
        container.replaceChildren(paragraph);
    };

    fetch(container.dataset.historyUrl, { headers: { Accept: "application/json" } })
        .then((response) => {
            if (!response.ok) throw new Error("Price history could not be loaded.");
            return response.json();
        })
        .then((points) => {
            if (!Array.isArray(points) || points.length === 0) {
                showMessage("No verified price history is available yet.");
                return;
            }

            const width = 720;
            const height = 260;
            const padding = { top: 24, right: 24, bottom: 42, left: 76 };
            const prices = points.map((point) => Number(point.price));
            const minimum = Math.min(...prices);
            const maximum = Math.max(...prices);
            const spread = maximum - minimum || 1;
            const x = (index) => padding.left + index * (width - padding.left - padding.right) / Math.max(points.length - 1, 1);
            const y = (price) => height - padding.bottom - (price - minimum) * (height - padding.top - padding.bottom) / spread;
            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            svg.setAttribute("viewBox", `0 0 ${width} ${height}`);
            svg.setAttribute("class", "price-history-svg");
            svg.setAttribute("role", "img");
            svg.setAttribute("aria-label", "Verified retailer price history");

            const axis = document.createElementNS(svg.namespaceURI, "path");
            axis.setAttribute("d", `M${padding.left} ${padding.top}V${height - padding.bottom}H${width - padding.right}`);
            axis.setAttribute("class", "chart-axis");
            svg.append(axis);

            const line = document.createElementNS(svg.namespaceURI, "polyline");
            line.setAttribute("points", points.map((point, index) => `${x(index)},${y(Number(point.price))}`).join(" "));
            line.setAttribute("class", "chart-line");
            svg.append(line);

            const first = document.createElementNS(svg.namespaceURI, "text");
            first.setAttribute("x", padding.left);
            first.setAttribute("y", height - 12);
            first.setAttribute("class", "chart-label");
            first.textContent = new Date(points[0].capturedAt).toLocaleDateString();
            svg.append(first);

            const last = document.createElementNS(svg.namespaceURI, "text");
            last.setAttribute("x", width - padding.right);
            last.setAttribute("y", height - 12);
            last.setAttribute("text-anchor", "end");
            last.setAttribute("class", "chart-label");
            last.textContent = new Date(points[points.length - 1].capturedAt).toLocaleDateString();
            svg.append(last);

            const high = document.createElementNS(svg.namespaceURI, "text");
            high.setAttribute("x", padding.left - 10);
            high.setAttribute("y", padding.top + 4);
            high.setAttribute("text-anchor", "end");
            high.setAttribute("class", "chart-label");
            high.textContent = `₹${maximum.toLocaleString()}`;
            svg.append(high);

            const low = document.createElementNS(svg.namespaceURI, "text");
            low.setAttribute("x", padding.left - 10);
            low.setAttribute("y", height - padding.bottom + 4);
            low.setAttribute("text-anchor", "end");
            low.setAttribute("class", "chart-label");
            low.textContent = `₹${minimum.toLocaleString()}`;
            svg.append(low);
            container.replaceChildren(svg);

            const table = document.createElement("table");
            table.className = "price-history-table";
            const body = document.createElement("tbody");
            points.slice(-10).reverse().forEach((point) => {
                const row = document.createElement("tr");
                [point.platformName, `₹${Number(point.price).toLocaleString()}`, new Date(point.capturedAt).toLocaleString()].forEach((value) => {
                    const cell = document.createElement("td");
                    cell.textContent = value;
                    row.append(cell);
                });
                body.append(row);
            });
            table.append(body);
            container.append(table);
        })
        .catch(() => showMessage("Price history is temporarily unavailable."));
})();
