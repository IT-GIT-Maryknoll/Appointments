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
    loadDriversScheduleCalendar();

    // Trigger re-render when standard checkbox changes
    if (checkbox) { // Added a quick safety check
        checkbox.addEventListener('change', (e) => {
            if (e.target.checked) {
                loadDriversScheduleCalendar();
            }
        });
    }
    if (checkbox1) {
        checkbox1.addEventListener('change', () => {
            if (typeof ec !== 'undefined' && ec) {
                ec.refetchEvents();
            } else {
                console.warn("Calendar is not initialized yet. Load it first!");
            }
        });
    }
});

function loadDriversScheduleCalendar() {
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
                    var { startIso, endIso, currentValue, driverValue } = newFunctiontest(fetchInfo, savedView);

                    const url = `/AppointmentDetails?handler=Appointments&start=${encodeURIComponent(startIso)}&end=${encodeURIComponent(endIso)}&sharedMessage=${encodeURIComponent(currentValue)}&driverName=${encodeURIComponent(driverValue)}`;

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
            document.getElementById('m-start').innerText = (escapeHtml(formatTime(e.start)) || ' ');
            document.getElementById('m-end').innerText = (escapeHtml(formatTime(e.end)) || ' ');
            document.getElementById('m-description').innerText = (escapeHtml(e.extendedProps.doctorAddress)) || ' ';
            modal.style.display = 'flex';
        }
    });
}
function newFunctiontest(fetchInfo, savedView) {
    const currentValue = document.getElementById("SharedMessage").value;
    const driverValue = document.getElementById("msgDriverName").value;
    let targetStart = new Date(fetchInfo.start);
    let targetEnd = new Date(fetchInfo.end);
    if (typeof ec !== 'undefined' && ec.getView) {
        let currentView = ec.getView();
        if (currentView.type === 'dayGridMonth') {
            let midpoint = new Date(targetStart.getTime() + (targetEnd.getTime() - targetStart.getTime()) / 2);
            let currentYear = midpoint.getFullYear();
            let currentMonth = midpoint.getMonth();
            targetStart = new Date(currentYear, currentMonth, 1);
            targetEnd = new Date(currentYear, currentMonth + 1, 1);
        }
    } else if (savedView === 'dayGridMonth') {
        let midpoint = new Date(targetStart.getTime() + (targetEnd.getTime() - targetStart.getTime()) / 2);
        let currentYear = midpoint.getFullYear();
        let currentMonth = midpoint.getMonth();
        targetStart = new Date(currentYear, currentMonth, 1);
        targetEnd = new Date(currentYear, currentMonth + 1, 1);
    }
    let startIso = targetStart.toISOString().split('T')[0]; // Format: YYYY-MM-DD
    let endIso = targetEnd.toISOString().split('T')[0];
    return { startIso, endIso, currentValue, driverValue };
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
        // 1. Guard clause: Ensure the calendar instance exists before doing anything
        if (typeof ec === 'undefined' || !ec.getView) {
            throw new Error("Calendar instance 'ec' is not initialized.");
        }

        const currentView = ec.getView();
        let targetStart = new Date(currentView.activeStart);
        let targetEnd = new Date(currentView.activeEnd);

        // 2. Adjust dates if viewing a full month grid
        if (currentView.type === 'dayGridMonth') {
            const midpoint = new Date(targetStart.getTime() + (targetEnd.getTime() - targetStart.getTime()) / 2);
            const currentYear = midpoint.getFullYear();
            const currentMonth = midpoint.getMonth();

            targetStart = new Date(currentYear, currentMonth, 1);
            targetEnd = new Date(currentYear, currentMonth + 1, 1);
        }

        // 3. Helper to format 'YYYY-MM-DD' using local time to prevent timezone shifting
        const formatLocalDate = (date) => {
            const offset = date.getTimezoneOffset();
            const localizedDate = new Date(date.getTime() - (offset * 60 * 1000));
            return localizedDate.toISOString().split('T')[0];
        };

        const startIso = formatLocalDate(targetStart);
        const endIso = formatLocalDate(targetEnd);

        // 4. Safely extract DOM values with fallbacks
        const sharedMessage = document.getElementById("SharedMessage")?.value || "";
        const driverName = document.getElementById("msgDriverName")?.value || "";
        const isCheckboxChecked = document.getElementById("ShowInhouse")?.checked || false; // Assuming checkbox1 is an ID

        // 5. Build URL using URLSearchParams to handle automatic encoding
        const params = new URLSearchParams({
            handler: 'Appointments',
            start: startIso,
            end: endIso,
            sharedMessage: sharedMessage,
            driverName: driverName
        });

        // 6. Fetch and handle response
        const response = await fetch(`/AppointmentDetails?${params.toString()}`);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        if (!Array.isArray(data)) {
            return [];
        }

        // 7. Filter results cleanly using optional chaining
        return data.filter(event => {
            const isInHouse = event.extendedProps?.inhouse ?? event.inhouse ?? false;
            return !isInHouse || isCheckboxChecked;
        });
    } catch (error) {
        console.error("Appointment fetch failed:", error.message);
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