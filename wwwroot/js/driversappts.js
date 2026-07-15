// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const modal = document.getElementById('eventModal');
const monthSelect = document.getElementById('calendar-month');
const yearSelect = document.getElementById('calendar-year');
let ec;
document.addEventListener('DOMContentLoaded', function () {
    //Handle window resize to adjust calendar view
    window.addEventListener('resize', () => {
        if (window.innerWidth < 700 && ec.getView().type === 'dayGridMonth') {
            ec.setOption('view', 'listWeek');
        }
    });
    window.onclick = function (event) {
        if (event.target === modal) closeModal();
    };
    // ---- Month & Year Select Setup ----
    // Populate months
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

    // Populate years (range: current - 10 to current + 5)
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 1; y <= currentYear + 5; y++) {
        const opt = document.createElement("option");
        opt.value = y;
        opt.textContent = y;
        yearSelect.appendChild(opt);
    }

    // Set defaults to today's date
    const today = new Date();
    monthSelect.value = today.getMonth();
    yearSelect.value = today.getFullYear();

    ec = new EventCalendar(document.getElementById('calendar'), {
        //   view: window.innerWidth < 600 ? 'listWeek' : 'dayGridMonth', // Responsive default view
        view: window.innerWidth < 600 ? 'listWeek' : 'listWeek', // Responsive default view
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

                    // Check if fetchInfo exists to prevent the crash
                    if (!fetchInfo || !fetchInfo.startStr) {
                        console.log("Calendar is still warming up...");
                        return;
                    }

                    //  const url = `/Appointment/FetchEvents?start=${fetchInfo.startStr}&end=${fetchInfo.endStr}`;
                    //const url = '/AppointmentDetails?handler=GetAppointments&start=${encodeURIComponent(fetchInfo.startStr)}&end=${encodeURIComponent(fetchInfo.endStr)}';
                    const url = `/AppointmentDetails?handler=GetAppointments&start=${fetchInfo.startStr}&end=${fetchInfo.endStr}`;
                    fetch(url)
                        .then(res => res.json())
                        .then(data => {

                            // Ensure we always return an array []
                            successCallback(Array.isArray(data) ? data : []);
                        })
                        .catch(err => {
                            console.error("Fetch error:", err);
                            failureCallback(err);
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
                hour: 'numeric',
                minute: 'numeric',
                hour12: true
            }) : "";

            const dpDepart = event.start ? new Date(event.start).toLocaleTimeString('en-US', {
                hour: 'numeric',
                minute: 'numeric',
                hour12: true
            }) : "";
            const dpAppt = event.end ? new Date(event.end).toLocaleTimeString('en-US', {
                hour: 'numeric',
                minute: 'numeric',
                hour12: true
            }) : "";

            // Extract your custom badge fields
          //  let badgeText = event.extendedProps ? event.extendedProps.badgeText : undefined;
            let badgeClass = event.extendedProps ? event.extendedProps.badgeClass : 'badge-default';
            const end = event.extendedProps.endTime || "";

            const desc = event.extendedProps.description || "";
            const loc = event.extendedProps.location || "";
            const day = event.startStr ? event.startStr.split("T")[0] : "";

            // MONTH VIEW
            if (view === "dayGridMonth") {
                // Check if a badge exists for this event
                if (event.extendedProps.badgeText==="I") {
                    return {
                        html: `<div class="ec-event-title">
                        <span class="custom-badge ${badgeClass}">${event.extendedProps.badgeText}</span>
                        ${title}        ${dpDepart} 
                       </div>`
                    };
                }

                // Default return if no badge is present
                return {
                    html: `<div class="ec-event-title">${title}  Depart ${dpDepart} Appt ${dpAppt}</div>`
                };

                //     return {
                //         html: `
                //    <div>${start}</div>

                // `
                //     };
            }

            // WEEK VIEW
            //             else if (view === "listWeek") {
            //                 return {
            //                     html: `
            //     <span class="ec-line">
            //         <strong>⏰ Time ${start} - ${end}</strong>
            //         <span class="sep">|</span>

            //          <strong>📝  ${title} </strong>
            //         <span class="sep">|</span>

            //         <strong> 📍 wwRoom ${loc}</strong>
            //         <span class="sep">|</span>

            //         <strong>  💬 Dept/Contact ${desc}</strong>
            //     </span>
            // `
            //                 };
            //             }

            // DAY VIEW
            else if ((view === "listDay" || view === "listWeek") && event.extendedProps.inhouse === true) {
                console.log("Listview");
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
            else if ((view === "listDay" || view === "listWeek") && event.extendedProps.inhouse === false) {
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
        },
        eventClick: function (info) {
            const e = info.event;
            const modal = document.getElementById('eventModal');

            document.getElementById('m-title').innerText = e.title;
            document.getElementById('m-date').innerText = e.start.toDateString();
            document.getElementById('m-location').innerText = /* "📍 " */  (e.extendedProps.location || 'Unknown Location');
            document.getElementById('m-start').innerText = (e.extendedProps.starttime || '');
            document.getElementById('m-end').innerText = (e.extendedProps.endTime || '');
            document.getElementById('m-description').innerText = e.extendedProps.description || 'No description provided.';

            modal.style.display = 'flex';
        }
    });
});
function closeModal() {
    modal.style.display = 'none';
}

document.getElementById("go-button").addEventListener("click", () => {
    const year = parseInt(yearSelect.value, 10);
    const month = parseInt(monthSelect.value, 10);

    // Move calendar to selected month/year
    ec.setOption("date", new Date(year, month, 1));
});