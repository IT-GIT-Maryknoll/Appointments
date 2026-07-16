// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const modal = document.getElementById('eventModal');
const monthSelect = document.getElementById('calendar-month');
const yearSelect = document.getElementById('calendar-year');
let ec;
const checkbox = document.querySelector('.form-check-input');
const checkbox1 = document.getElementById("ShowInhouse");

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
});

function newFunction() {
    let savedView = window.innerWidth < 600 ? 'listWeek' : 'dayGridMonth'; // Your fallback default
    let savedDate = new Date();
    if (ec) {
        savedView = ec.getOption('view');
        savedDate = ec.getOption('date');
        console.log(ec.view);
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
                    // window.print();
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
                    const url = `/AppointmentDetails?handler=GetAppointments&start=${fetchInfo.startStr}&end=${fetchInfo.endStr}`;
                    fetch(url)
                        .then(res => res.json())
                        .then(data => {
                            if (Array.isArray(data)) {
                                // Filter out events where inhouse is true IF checkbox1 is unchecked
                                const filteredData = data.filter(event => {
                                    if (event.extendedProps && event.extendedProps.inhouse === true) {
                                        return checkbox1.checked; // keeps if checked, removes if unchecked
                                    }
                                    // Alternative check if "inhouse" is at the root level of your JSON object:
                                    if (event.inhouse === true) {
                                        return checkbox1.checked;
                                    }
                                    return true; // Keep all other events
                                });
                                successCallback(filteredData);
                            } else {
                                successCallback([]);
                            }
                        });
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

            // MONTH VIEW
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

            // LIST & DAY VIEWS: Handle logic based on checkboxes dynamically
            if (view === "listDay" || view === "listWeek") {
                // CRITICAL FIX: If event is inhouse, but "Show Inhouse" checkbox is false, hide the event structure
                if (event.extendedProps.inhouse === true) {
                    if (checkbox1.checked === true) {
                        return newFunction_1(start, title, event);
                    } else {
                        // Returning an empty object or hidden div safely hides value mapping in EventCalendar UI elements
                        return { html: `<div style="display:none;"></div>` };
                    }
                }

                // Fallback for regular non-inhouse appointments
                if (event.extendedProps.inhouse === false) {
                    return newFunction_2(dpDepart, dpAppt, title, event);
                }
            }
        },
        eventClick: function (info) {
            const e = info.event;
            document.getElementById('m-title').innerText = e.title;
            document.getElementById('m-date').innerText = e.start.toDateString();
            document.getElementById('m-location').innerText = (e.extendedProps.location || 'Unknown Location');
            document.getElementById('m-start').innerText = (e.extendedProps.starttime || '');
            document.getElementById('m-end').innerText = (e.extendedProps.endTime || '');
            document.getElementById('m-description').innerText = (e.extendedProps.description || '');
        }
    });
}

function newFunction_2(dpDepart, dpAppt, title, event) {
    return {
        html: `
<span class="ec-line">
	<strong>⏰ Depart:</strong> ${dpDepart} <strong>Appt:</strong> ${dpAppt}
	<strong>📝Resident:</strong> ${title}
	<strong>📍Doctor:</strong> ${event.extendedProps.doctorName}
	<strong>💬Wait:</strong> ${event.extendedProps.wait ? 'Yes' : 'No'}
</span>
<span class="ec-line">
	<strong>📍Address:</strong> ${event.extendedProps.doctorAddress}
</span>           `
    };
}

function newFunction_1(start, title, event) {
    return {
        html: `
            <span class="ec-line">
	<strong>⏰Appt:</strong> ${start}
	<strong>📝Resident:</strong> ${title}
	<strong>📍Doctor:</strong> ${event.extendedProps.doctorName}
	<strong>💬Wait:</strong> ${event.extendedProps.wait ? 'Yes' : 'No'}
</span>
<span class="ec-line">
	<strong>📍Address:</strong>${event.extendedProps.doctorAddress}
</span>          `
    };
}

function printAppointmentTable() {
    const titleText = document.querySelector('.ec-title')?.innerText || 'Appointment Schedule';
    const dayElements = document.querySelectorAll('.ec-day');
    const appointmentsByDay = [];

    dayElements.forEach(dayEl => {
        const dayHead = dayEl.querySelector('.ec-day-head');
        if (!dayHead) return;

        const dateText = dayHead.innerText.replace(/\s+/g, ' ').trim();
        const events = dayEl.querySelectorAll('.ec-event');

        events.forEach(eventEl => {
            const lines = eventEl.querySelectorAll('.ec-line');
            if (lines.length === 0) return;

            let mainLineText = lines[0].innerText || lines[0].textContent;
            let addressText = lines[1] ? (lines[1].innerText || lines[1].textContent) : '';

            // Clean prefixes and icons out of text fields
            mainLineText = mainLineText.replace(/[⏰📝📍💬]/g, '');
            addressText = addressText.replace(/[📍]/g, '').replace(/Address:\s*/i, '').trim();

            // Extract distinct parameter tokens accurately
            const departMatch = mainLineText.match(/Depart:\s*([^\n\r]+?)(?=\s*Appt:|$)/i);
            const apptMatch = mainLineText.match(/Appt:\s*([^\n\r]+?)(?=\s*Resident:|$)/i);
            const residentMatch = mainLineText.match(/Resident:\s*([^\n\r]+?)(?=\s*Doctor:|$)/i);
            const doctorMatch = mainLineText.match(/Doctor:\s*([^\n\r]+?)(?=\s*Wait:|$)/i);
            const waitMatch = mainLineText.match(/Wait:\s*([^\n\r]+)$/i);

            appointmentsByDay.push({
                date: dateText,
                depart: departMatch ? departMatch[1].trim() : '—',
                appt: apptMatch ? apptMatch[1].trim() : (mainLineText.match(/Appt:\s*([^\n\r]+?)(?=\s*Resident:|$)/i) ? '' : mainLineText.split('Resident:')[0].replace(/Appt:\s*/i, '').trim()),
                resident: residentMatch ? residentMatch[1].trim() : '',
                doctor: doctorMatch ? doctorMatch[1].trim() : '—',
                wait: waitMatch ? waitMatch[1].trim() : 'No',
                address: addressText
            });
        });
    });

    if (appointmentsByDay.length === 0) {
        alert("No appointments found to print.");
        return;
    }

    let printContent = `
    <html>
    <head>
        <title>${titleText}</title>
        <style>
            body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Arial, sans-serif; margin: 30px; color: #333; }
            h2 { text-align: center; margin-bottom: 5px; color: #111; }
            h3 { text-align: center; margin-top: 0; font-weight: normal; color: #666; font-size: 16px; }
            table { width: 100%; border-collapse: collapse; margin-top: 25px; }
            th, td { border: 1px solid #ddd; padding: 10px; text-align: left; font-size: 13px; vertical-align: top; }
            th { background-color: #f5f5f5; font-weight: bold; color: #222; }
            tr:nth-child(even) { background-color: #fafafa; }
            .date-cell { font-weight: 600; color: #000; white-space: nowrap; }
        </style>
    </head>
    <body>
        <h2>Appointment Schedule Table</h2>
        <h3>${titleText}</h3>
        <table>
            <thead>
                <tr>
                    <th>Day / Date</th>
                    <th>Depart Time</th>
                    <th>Appt Time</th>
                    <th>Resident</th>
                    <th>Doctor</th>
                    <th>Wait</th>
                    <th>Address</th>
                </tr>
            </thead>
            <tbody>
    `;

    appointmentsByDay.forEach(appt => {
        printContent += `
            <tr>
                <td class="date-cell">${appt.date}</td>
                <td>${appt.depart}</td>
                <td>${appt.appt}</td>
                <td>${appt.resident}</td>
                <td>${appt.doctor}</td>
                <td>${appt.wait}</td>
                <td>${appt.address}</td>
            </tr>
        `;
    });

    printContent += `
            </tbody>
        </table>
    </body>
    </html>
    `;

    const printWindow = window.open('', '_blank', 'height=700,width=900');
    printWindow.document.write(printContent);
    printWindow.document.close();

    printWindow.focus();
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 300);
}