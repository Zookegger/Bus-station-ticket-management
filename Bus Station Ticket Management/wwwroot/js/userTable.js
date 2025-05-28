document.addEventListener("DOMContentLoaded", function () {
	const tableSelector = "#DataTable";
	const detailsUrl = document.querySelector(tableSelector).dataset.detailsUrl;
	const tableName = document.querySelector(tableSelector).dataset.tableName;
	const tableTitle = document.querySelector(tableSelector).dataset.tableTitle;

	initializeDataTable(tableTitle, tableSelector, {
		ajax: null, // Loads data via AJAX
		columns: null, // Defines column data sources
		order: [[0, "asc"]], // Default column ordering
		rowCallback: null, // Callback for row creation
		initComplete: function () {
			// Find the collection button and fix its classes
			$(".buttons-collection")
				.removeClass("btn-secondary") // remove default solid styling
				.addClass("btn-outline-secondary"); // ensure outline is applied

			tippy("[data-tippy-content]", {
				placement: "top",
				theme: "light",
			});

			// Re-initialize Tippy every time the collection dropdown is shown
			$(document).on("click", ".buttons-collection", function () {
				// Timeout allows the dropdown to render its DOM
				setTimeout(function () {
					tippy("[data-tippy-content]", {
						placement: "top",
						theme: "light",
						delay: [100, 0],
					});
				}, 10);
			});

			$(".dt-length").attr(
				"data-tippy-content",
				"Choose how many entries to display"
			);
			$(".dt-search").attr(
				"data-tippy-content",
				`Search for ${tableTitle} in the database`
			);
		},
	});

	if (document.querySelector("#detailsOffcanvas") && document.querySelector("#detailsOffcanvasBody")) {
		initializeRowClickHandler(
			tableSelector,
			detailsUrl,
			"#detailsOffcanvas",
			"#detailsOffcanvasBody"
		);
	} else {
		initializeRowClickHandler(
			tableSelector,
			detailsUrl
		);
	}


	// Function to check if the table height is too small and toggle button visibility
	function adjustButtonVisibility() {
		const table = document.querySelector("#DataTable");
		if (!table) return;

		// Use timeout to wait for DOM render (rows must have height)
		setTimeout(function () {
			const firstRow = table.querySelector("tbody tr");
			if (!firstRow) return;

			const rowHeight = firstRow.offsetHeight;
			const minTableHeight = 4 * rowHeight;

			const tableHeight = table.offsetHeight;

			const useSimpleButtons = tableHeight < minTableHeight;

			document
				.querySelectorAll(".dropdown-buttons")
				.forEach(function (btnGroup) {
					btnGroup.style.display = useSimpleButtons ? "none" : "flex";
				});
			document
				.querySelectorAll(".simple-buttons")
				.forEach(function (btnGroup) {
					btnGroup.style.display = useSimpleButtons
						? "block"
						: "none";
				});
		}, 100); // Wait briefly to ensure rows are fully rendered
	}

	// Initial call to adjust visibility
	adjustButtonVisibility();

	// Adjust visibility on window resize (optional)
	window.addEventListener("resize", function () {
		adjustButtonVisibility();
	});

	tippy("[data-tippy-content]", {
		placement: "top",
		theme: "light",
	});
});

function initializeDataTable(tableTitle, selector, options) {
	console.log(tableTitle);

	const defaultOptions = {
		ordering: true,
		paging: true,
		searching: true,
		info: true,
		filter: true,
		responsive: false,
		columnDefs: [
			{
				targets: -1,
				orderable: false,
			},
		],
		autoWidth: true,
		deferRender: true,
		lengthChange: true,
		lengthMenu: [10, 25, 50, 100],
		pageLength: 10,
		processing: true,
		scrollX: false,
		scrollY: "600px",
		scrollCollapse: true,
		stateSave: false,
		language: {
			search: `Search for ${tableTitle}:`,
			lengthMenu: "Show _MENU_ entries",
			info: "_START_ to _END_ of _TOTAL_ entries",
		},
		dom:
			"<'row my-3 px-3' \
                <'col-12 col-sm-12 col-md-4 d-flex align-items-center justify-content-start px-0'l> \
                <'col-12 col-sm-10 col-md-7 d-flex align-items-center justify-content-sm-start justify-content-start px-0 pe-md-2'f> \
                <'col-12 col-sm-2 col-md-1 d-flex align-items-center justify-content-end px-0 ps-md-2'B> \
            >" +
			"<'px-3'rt>" +
			"<'row px-3 py-3'" +
			"<'col-md-6'i>" +
			"<'col-md-6'p>" +
			">",
		buttons: [
			{
				extend: "collection",
				text: '<i class="fa fa-ellipsis-h"></i>',
				className: "btn btn-outline-secondary",
				attr: {
					"data-tippy-content": "More actions",
				},
				buttons: [
					{
						extend: "copy",
						text: "Copy",
						attr: { "data-tippy-content": "Copy table data" },
						exportOptions: { columns: ":not(:last-child)" },
					},
					{
						extend: "csv",
						text: "CSV",
						attr: { "data-tippy-content": "Export to CSV" },
						exportOptions: { columns: ":not(:last-child)" },
					},
					{
						extend: "excel",
						text: "Excel",
						attr: { "data-tippy-content": "Export to Excel" },
						exportOptions: { columns: ":not(:last-child)" },
					},
					{
						extend: "pdf",
						text: "PDF",
						attr: { "data-tippy-content": "Export to PDF" },
						exportOptions: { columns: ":not(:last-child)" },
					},
					{
						extend: "print",
						text: "Print",
						attr: { "data-tippy-content": "Print table data" },
						exportOptions: { columns: ":not(:last-child)" },
					},
				],
			},
		],
	};

	$(selector).DataTable($.extend(true, {}, defaultOptions, options));
}

function initializeRowClickHandler(
	tableSelector,
	detailsUrl,
	offcanvasSelector,
	offcanvasBodySelector
) {
	$(tableSelector).on("click", ".clickable-row", function () {
		const id = $(this).data("id");

		if (!id) {
			alert("No ID found! Make sure data-id is set properly.");
			return;
		}
		if (
			offcanvasSelector === undefined ||
			offcanvasBodySelector === undefined
		) {
			window.location.href = `${detailsUrl}?id=${id}`;
			return;
		} else {
			$(offcanvasBodySelector).html(
				'<div class="text-center p-3"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>'
			);

			$.get(`${detailsUrl}?id=${id}`, function (data) {
				$(offcanvasBodySelector).html(data);
				const offcanvas = new bootstrap.Offcanvas(
					document.querySelector(offcanvasSelector)
				);
				offcanvas.show();
			}).fail(function () {
				$(offcanvasBodySelector).html(
					`<div class="alert alert-danger">Failed to load ${tableTitle} details.</div>`
				);
			});
		}
	});
}
