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

    // // CRITICAL FIX: Re-render the calendar when the Inhouse checkbox shifts
    // checkbox1.addEventListener('change', () => {
    //     newFunction();
    // });
});

function newFunction() {
    if (ec) {
        ec.destroy();
    }

    ec = new EventCalendar(document.getElementById('calendar'), {
        view: window.innerWidth < 600 ? 'listWeek' : 'listWeek',
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
                    window.print();
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
	<strong>⏰ Depart: ${dpDepart} Appt: ${dpAppt}  </strong>
	<span class="sep">|</span>
	<strong>📝Resident: ${title} </strong>
	<span class="sep">|</span>
	<strong>📍 Doctor: ${event.extendedProps.doctorName}</strong>
	<span class="sep">|</span>
</span>
<span class="ec-line">
	<strong> 💬 Wait: ${event.extendedProps.wait ? 'Yes' : 'No'}</strong>
	<span class="sep">|</span>
	<strong> 📍 Address: ${event.extendedProps.doctorAddress}</strong>
	<span class="sep"></span>
</span>             `
    };
}

function newFunction_1(start, title, event) {
    return {
        html: `
               <span class="ec-line">
	<strong>⏰ Appt ${start}</strong>
	<span class="sep">|</span>
	<strong>📝Resident ${title} </strong>
	<span class="sep">|</span>
	<strong>📍 Doctor ${event.extendedProps.doctorName}</strong>
	<span class="sep">|</span>
</span>
<span class="ec-line">
	<strong> 💬 Wait: ${event.extendedProps.wait ? 'Yes' : 'No'}</strong>
	<span class="sep">|</span>
	<strong> 📍 Address ${event.extendedProps.doctorAddress}</strong>
	<span class="sep">|</span>
</span>             `
    };
}
