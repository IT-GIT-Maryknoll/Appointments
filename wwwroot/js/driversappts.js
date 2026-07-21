const modal = document.getElementById('eventModal');
const monthSelect = document.getElementById('calendar-month');
const yearSelect = document.getElementById('calendar-year');
let ec;
const checkbox = document.querySelector('.form-check-input');
const checkbox1 = document.getElementById("ShowInhouse");
let sortedList = null;
let filteredData = null;

document.addEventListener('DOMContentLoaded', function () {
    // Handle window resize to adjust calendar view
    window.addEventListener('resize', () => {
        if (window.innerWidth < 700 && ec.getView().type === 'dayGridMonth') {
            ec.setOption('view', 'listWeek');
        }
    });

    window.onclick = function (event) {
        if (event.target === modal) closeModal();
    };

    // ---- Month & Year Select Setup ----
    const monthNames = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    monthNames.forEach((m, i) => {
        const opt = document.createElement("option");
        opt.value = i;   // 0–11 for JS date
        opt.textContent = m;
        monthSelect.appendChild(opt);
    });

    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 1; y <= currentYear + 5; y++) {
        const opt = document.createElement("option");
        opt.value = y;
        opt.textContent = y;
        yearSelect.appendChild(opt);
    }

    const today = new Date();
    monthSelect.value = today.getMonth();
    yearSelect.value = today.getFullYear();

    // Render calendar for the first time
    newFunction();

    // Trigger re-render when standard checkbox changes
    checkbox.addEventListener('change', () => {
        newFunction();
    });

    // Trigger calendar refresh when "Show Inhouse" checkbox status changes
    checkbox1.addEventListener('change', () => {
        if (ec) {
            ec.refetchEvents();
        }
    });
});

function newFunction() {
    let savedView = window.innerWidth < 600 ? 'listWeek' : 'dayGridMonth';
    let savedDate = new Date();
    if (ec) {
        savedView = ec.getOption('view');
        savedDate = ec.getOption('date');
        ec.destroy();
    }
    ec = new EventCalendar(document.getElementById('calendar'), {
        view: savedView,
        date: savedDate,
        height: '100%',
        firstday: 1,
        eventStartEditable: false,
        headerToolbar: {
            start: 'prev,next,today,print',
            center: 'title',
            end: 'dayGridMonth,listDay,listWeek ',
        },
        buttonText: {
            listDay: 'Day',
            dayGridMonth: 'Month',
            listWeek: 'Week',
            today: 'Today'
        },
        customButtons: {
            print: {
                text: 'Print',
                click: function () {
                    printAppointmentTable();
                }
            }
        },
        events: [],
        eventSources: [
            {
                events: function (fetchInfo, successCallback, failureCallback) {
                    if (!fetchInfo || !fetchInfo.startStr) {
                        console.log("Calendar is still warming up...");
                        return;
                    }
                    const url = `/AppointmentDetails?handler=GetAppointments&start=${fetchInfo.startStr.split('T')[0]}&end=${fetchInfo.endStr.split('T')[0]}`;
                    getAppDateDetails(fetchInfo, successCallback, url);
                },
            }
        ],

        eventContent(info) {
            const event = info.event;
            const title = event.title || "";
            info.event.backgroundColor = '#c4eda9';
            const view = info.view.type;

            const start = event.extendedProps.apptime ? new Date(event.extendedProps.apptime).toLocaleTimeString('en-US', {
                hour: 'numeric', minute: 'numeric', hour12: true
            }) : "";

            const dpDepart = event.start ? new Date(event.start).toLocaleTimeString('en-US', {
                hour: 'numeric', minute: 'numeric', hour12: true
            }) : "";

            const dpAppt = event.end ? new Date(event.end).toLocaleTimeString('en-US', {
                hour: 'numeric', minute: 'numeric', hour12: true
            }) : "";

            let badgeClass = event.extendedProps ? event.extendedProps.badgeClass : 'badge-default';

            if (view === "dayGridMonth") {
                if (event.extendedProps.badgeText === "I") {
                    return {
                        html: `<div class="ec-event-title">
                        <span class="custom-badge ${badgeClass}">${event.extendedProps.badgeText}</span>
                        ${title}       ${dpDepart}
                       </div>`
                    };
                }
                return {
                    html: `<div class="ec-event-title">${title}  Depart ${dpDepart} Appt ${dpAppt}</div>`
                };
            }

            if (view === "listDay" || view === "listWeek") {
                if (event.extendedProps.inhouse === true) {
                    if (checkbox1.checked === true) {
                        return newFunction_1(start, title, event);
                    } else {
                        return { html: `<div style="display:none;"></div>` };
                    }
                }

                if (event.extendedProps.inhouse === false) {
                    return newFunction_2(dpDepart, dpAppt, title, event);
                }
            }
        },
        eventClick: function (info) {
            const e = info.event;
           
            const modal = document.getElementById('eventModal');

            document.getElementById('m-title').innerText = e.title;
            document.getElementById('m-date').innerText = e.start.toDateString();
            document.getElementById('m-location').innerText = /* "📍 " */  (escapeHtml(e.extendedProps.driverName) || ' ');
            document.getElementById('m-start').innerText = (escapeHtml(formatTime(e.start ))|| ' ');
            document.getElementById('m-end').innerText = (escapeHtml(formatTime(e.end ))|| ' ');
            document.getElementById('m-description').innerText = (escapeHtml(e.extendedProps.doctorAddress)) || ' ';

            modal.style.display = 'flex';
         

        }
    });
}
function closeModal() {
    modal.style.display = 'none';
}

function getAppDateDetails(fetchInfo, successCallback, url) {
    fetch(url)
        .then(res => res.json())
        .then(data => {
            if (Array.isArray(data)) {
                filteredData = data.filter(event => {
                    const isInHouse = event.extendedProps?.inhouse === true || event.inhouse === true;
                    if (isInHouse) {
                        return checkbox1.checked;
                    }
                    return true;
                });
                successCallback(filteredData);
            } else {
                successCallback([]);
            }
        });
}

function newFunction_1(start, title, event) {
    return {
        html: `
        <span class="ec-line">
            <strong>⏰Appt:</strong> ${start}
            <strong>📝Resident:</strong> ${title}
            <strong>📍Doctor:</strong> ${event.extendedProps.doctorName || ''}
            <strong>💬Wait:</strong> ${event.extendedProps.wait || ''}
            <strong>📍Driver:</strong> ${event.extendedProps.driverName || ''}
        </span>
        <span class="ec-line">
            <strong>📍Address:</strong> ${event.extendedProps.doctorAddress || ''}
        </span>`
    };
}

function newFunction_2(dpDepart, dpAppt, title, event) {
    return {
        html: `
        <span class="ec-line">
            <strong>⏰ Depart:</strong> ${dpDepart} <strong>Appt:</strong> ${dpAppt}
            <strong>📝Resident:</strong> ${title}
            <strong>📍Doctor:</strong> ${event.extendedProps.doctorName || ''}
            <strong>💬Wait:</strong> ${event.extendedProps.wait || ''}
            <strong>📍Driver:</strong> ${event.extendedProps.driverName || ''}
        </span>
        <span class="ec-line">
            <strong>📍Address:</strong> ${event.extendedProps.doctorAddress || ''}
        </span>`
    };
}

function getAppDateDetailsForPrint() {
    const currentView = ec.getView();
    const formatDateLocal = (dateObj) => {
        const d = new Date(dateObj);
        const month = '' + (d.getMonth() + 1);
        const day = '' + d.getDate();
        const year = d.getFullYear();
        return [year, month.padStart(2, '0'), day.padStart(2, '0')].join('-');
    };

    const startStr = formatDateLocal(currentView.activeStart);
    const endStr = formatDateLocal(currentView.activeEnd);

    const url = `/AppointmentDetails?handler=GetAppointments&start=${startStr}&end=${endStr}`;
    return fetch(url)
        .then(res => res.json())
        .then(data => {
            if (!Array.isArray(data)) return [];

            return data.filter(event => {
                const isInHouse = event.extendedProps?.inhouse || event.inhouse;
                return !isInHouse || checkbox1.checked;
            });
        })
        .catch(() => []);
}

async function printAppointmentTable() {
    const titleText =
        document.querySelector(".ec-title")?.textContent?.trim() ||
        "Appointment Schedule";

    const printWindow = window.open("", "_blank");

    if (!printWindow) {
        alert("Popup blocked. Please allow popups to print.");
        return;
    }

    try {
        let events = await getAppDateDetailsForPrint();
        events = sortAppointmentsByStart(events);

        const rows = events.map(event => {
            const departDate = new Date(event.start);
            const apptDate = event.extendedProps?.apptime
                ? new Date(event.extendedProps.apptime)
                : null;

            return `
                <tr>
                    <td class="date-cell">${escapeHtml(formatDate(departDate))}</td>
                    <td>${escapeHtml(formatTime(departDate))}</td>
                    <td>${escapeHtml(formatTime(apptDate))}</td>
                    <td>${escapeHtml(event.title)}</td>
                    <td>${escapeHtml(event.extendedProps?.doctorName)}</td>
                    <td>${escapeHtml(event.extendedProps?.driverName)}</td>
                    <td>${escapeHtml(event.extendedProps?.wait)}</td>
                    <td>${escapeHtml(event.extendedProps?.doctorAddress)}</td>
                </tr>
            `;
        }).join("");

        const html = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>${escapeHtml(titleText)}</title>
            <style>
                body {
                    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Arial, sans-serif;
                    margin: 20px;
                    color: #333;
                }

                h2 {
                    text-align: center;
                    margin-bottom: 5px;
                }

                h3 {
                    text-align: center;
                    font-weight: normal;
                    margin-top: 0;
                    color: #666;
                }

                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 20px;
                }

                th, td {
                    border: 1px solid #ddd;
                    padding: 8px;
                    text-align: left;
                    vertical-align: top;
                    font-size: 12px;
                }

                th {
                    background: #f5f5f5;
                }

                .date-cell {
                    white-space: nowrap;
                    font-weight: 600;
                }

                td:last-child {
                    max-width: 300px;
                    word-break: break-word;
                }

                @media print {
                    body {
                        margin: 10px;
                    }
                }
            </style>
        </head>
        <body>
            <h2>Appointment Schedule Table</h2>
            <h3>${escapeHtml(titleText)}</h3>

            <table>
                <thead>
                    <tr>
                        <th>Day / Date</th>
                        <th>Depart Time</th>
                        <th>Appt Time</th>
                        <th>Resident</th>
                        <th>Doctor</th>
                        <th>Driver</th>
                        <th>Wait</th>
                        <th>Address</th>
                    </tr>
                </thead>
                <tbody>
                    ${rows}
                </tbody>
            </table>
        </body>
        </html>`;

        printWindow.document.open();
        printWindow.document.write(html);
        printWindow.document.close();

        printWindow.onload = () => {
            printWindow.focus();
            printWindow.print();
            printWindow.close();
        };
    } catch (error) {
        console.error("Print failed:", error);
        printWindow.close();
        alert("Unable to generate appointment report.");
    }
}

async function getAppDateDetailsForPrint() {
    try {
        const start = ec.getView().activeStart
            .toISOString()
            .split("T")[0];

        const end = ec.getView().activeEnd
            .toISOString()
            .split("T")[0];

        const response = await fetch(
            `/AppointmentDetails?handler=GetAppointments&start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`
        );

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();

        if (!Array.isArray(data)) {
            return [];
        }

        return data.filter(event => {
            const isInHouse =
                event.extendedProps?.inhouse ??
                event.inhouse ??
                false;

            return !isInHouse || checkbox1.checked;
        });
    } catch (error) {
        console.error("Appointment fetch failed:", error);
        return [];
    }
}

function sortAppointmentsByStart(appointments, ascending = true) {
    return [...appointments].sort((a, b) => {
        const aTime = new Date(a.start).getTime();
        const bTime = new Date(b.start).getTime();

        if (Number.isNaN(aTime) && Number.isNaN(bTime)) return 0;
        if (Number.isNaN(aTime)) return 1;
        if (Number.isNaN(bTime)) return -1;

        return ascending ? aTime - bTime : bTime - aTime;
    });
}

function formatTime(date) {
    if (!(date instanceof Date) || Number.isNaN(date.getTime())) {
        return "";
    }

    return date.toLocaleTimeString("en-US", {
        hour: "numeric",
        minute: "2-digit",
        hour12: true
    });
}

function formatDate(date) {
    if (!(date instanceof Date) || Number.isNaN(date.getTime())) {
        return "";
    }

    return date.toLocaleDateString("en-US", {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric"
    });
}

function escapeHtml(value = "") {
    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

document.getElementById("go-button").addEventListener("click", () => {
    const year = parseInt(yearSelect.value, 10);
    const month = parseInt(monthSelect.value, 10);

// Move calendar to selected month/year
    ec.setOption("date", new Date(year, month, 1));
});