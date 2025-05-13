let tableTitleGlobal = null;


document.addEventListener("DOMContentLoaded", function () {
	const tableSelector = "#DataTable";
	const detailsUrl = document.querySelector(tableSelector).dataset.detailsUrl;
	const tableName = document.querySelector(tableSelector).dataset.tableName;
	const tableTitle = document.querySelector(tableSelector).dataset.tableTitle;

	tableTitleGlobal = tableTitle;

	initializeDataTable(tableName, tableTitle, tableSelector, {
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

            $(".dt-length").attr("data-tippy-content", "Choose how many entries to display");
            $(".dt-search").attr("data-tippy-content", `Search for ${tableTitle} in the database`);
		},
	});

	initializeRowClickHandler(
		tableSelector,
		detailsUrl,
		"#detailsOffcanvas",
		"#detailsOffcanvasBody"
	);

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

		document.querySelectorAll(".dropdown-buttons").forEach(function (btnGroup) {
			btnGroup.style.display = useSimpleButtons ? "none" : "flex";
		});
		document.querySelectorAll(".simple-buttons").forEach(function (btnGroup) {
			btnGroup.style.display = useSimpleButtons ? "block" : "none";
		});
	}, 100); // Wait briefly to ensure rows are fully rendered
}

function fetchDetails(controllerName, showMap = false, showRoute = false) {
	try {
	
		$('.clickable-row').on('click', function () {
			const id = $(this).data('id');

			$.ajax({
				url: `/Admin/${controllerName}/DetailsPartial?id=${id}`,
				method: 'GET',
				success: function (data) {
					$('#detailsOffcanvasBody').html(data);
					if (showMap) {
						// Initialize map after showing
						setTimeout(async function() {
							var latitude = $('#locationDetailsData').data('latitude');
							var longitude = $('#locationDetailsData').data('longitude');
							var name = $('#locationDetailsData').data('name');
							if (latitude && longitude && name) {
								initializeMap(latitude, longitude, name, false, 16);
							}
						}, 500);
					}
					if (showRoute) {
						// Initialize map after showing
						setTimeout(async function() {
							var startLatitude = parseFloat($('#routeDetailsData').data('start-latitude'));
							var startLongitude = parseFloat($('#routeDetailsData').data('start-longitude'));
							var endLatitude = parseFloat($('#routeDetailsData').data('end-latitude'));
							var endLongitude = parseFloat($('#routeDetailsData').data('end-longitude'));
							var startName = $('#routeDetailsData').data('start-name');
							var endName = $('#routeDetailsData').data('end-name');
	
							await initializeMap(startLatitude, startLongitude, startName, false);
							const { startLocation, endLocation } = await addRouteAndTime(startLatitude, startLongitude, endLatitude, endLongitude);
							
							if (startLocation && endLocation) {
								$('#startAddress').text(startLocation.display_name);
								$('#endAddress').text(endLocation.display_name);
							} else {
								console.warn(startLocation);
								console.warn(endLocation);
							}
						}, 100);
					}
				},
				error: function (xhr, status, error) {
					console.error('Error loading details:', error);
				}
			});
		});
	
	} catch (error) {
		console.error('Error fetching details:', error);
	}
}

function initializeDataTable(tableName, tableTitle, selector, options) {
	const defaultOptions = {
		ordering: true,
		colReorder: true,
		paging: true,
		searching: true,
		info: true,
		filter: true,
		responsive: false,
		autoWidth: true,
		deferRender: true,
		lengthChange: true,
		lengthMenu: [
			[10, 25, 50, 100, -1],
			[10, 25, 50, 100, 'All']
		],
		pageLength: -1,
		processing: true,
		scrollX: false,
		// scrollY: "600px",
		scrollY: false,
		scrollCollapse: true,
		stateSave: false,
		language: {
			search: 'Search:',
			lengthMenu: "Show _MENU_ entries",
			info: "_START_ to _END_ of _TOTAL_ entries",
		},
		dom:
			"<'#tableLayoutHeader.row my-3 px-3 w-100' \
				<'#showEntries.col-5 col-sm-12 col-md-5 d-flex align-items-center justify-content-start px-0' l> \
				<'#searchBar.col-5 col-sm-10 col-md-5 d-flex align-items-center justify-content-sm-start justify-content-end px-0 pe-md-2' f> \
				<'#moreButtons.col-2 col-sm-2 col-md-2 d-flex align-items-center justify-content-end px-0 ps-md-2' B> \
			>" +
			
			"<'#tableLayoutMain.px-3' rt>" +

			"<'#tableLayoutFooter.row px-3 py-3' \
				<'col-md-6' i> \
				<'col-md-6' p> \
			>",
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
						text: '<i class="fa-solid fa-copy fs-5 me-1" style="min-width: 22.5px;"></i> Copy',
						attr: { "data-tippy-content": "Copy table data" },
						exportOptions: { columns: ':not(:last-child)' },
					},
					{
						extend: "csv",
						text: '<i class="fa-solid fa-file-csv fs-5 me-1" style="min-width: 22.5px; color:rgb(3, 100, 191)"></i> CSV',
						attr: { "data-tippy-content": "Export to CSV" },
						exportOptions: { columns: ':not(:last-child)' },
					},
					{
						extend: "excel",
						text: '<i class="fa-solid fa-file-excel fs-5 me-1" style="min-width: 22.5px; color: #32a852"></i> Excel',
						attr: { "data-tippy-content": "Export to Excel" },
						exportOptions: { columns: ':not(:last-child)' },
					},
					{
						extend: "pdf",
						text: '<i class="fa-solid fa-file-pdf fs-5 me-1" style="min-width: 22.5px; color: #d33333"></i> PDF',
						attr: { "data-tippy-content": "Export to PDF" },
						exportOptions: { columns: ':not(:last-child)' },
					},
					{
						extend: "print",
						text: '<i class="fa-solid fa-print fs-5 me-1" style="min-width: 22.5px; color: #000000"></i> Print',
						attr: { "data-tippy-content": "Print table data" },
						exportOptions: { columns: ':not(:last-child)' },
					},
				],
			},
		],
	};
	// console.log(tableName);

	if (tableName !== "Revenue") {
		defaultOptions.columnDefs = [
			{
				targets: -1,
				orderable: false,
			},
		];
	}

	$(selector).DataTable($.extend(true, {}, defaultOptions, options));
}

function initializeRowClickHandler(tableSelector, detailsUrl, offcanvasSelector, offcanvasBodySelector) {
	$(tableSelector).on("click", ".clickable-row", function () {
		const id = $(this).data("id");
		// console.log(detailsUrl);
		if (!id) {
			alert("No ID found! Make sure data-id is set properly.");
			return;
		}

		$(offcanvasBodySelector).html(
			'<div class="text-center p-3"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>'
		);

		const offcanvas = bootstrap.Offcanvas.getOrCreateInstance(document.querySelector(offcanvasSelector), {
			toggle: true,
		});
		offcanvas.show();
		
		$.get(`${detailsUrl}?id=${id}`, function (data) {
			$(offcanvasBodySelector).html(data);
		}).fail(function () {
			$(offcanvasBodySelector).html(
				`<div class="alert alert-danger">Failed to load ${tableTitleGlobal} details.</div>`
			);
		});
	});
}