import axios from "axios";
import { objectToQueryString } from "../helpers/webrequest";
const baseUrl = `https://localhost:7047/api/PoliceIncident`;

export const kentPoliceService = {
    getIncidentsForTypeAndDate: getIncidentsForTypeAndDate,
};

function getIncidentsForTypeAndDate(kentPoliceIncidentTypeId, startDate, endDate) { 
    const params = {
        incidentTypeId: kentPoliceIncidentTypeId,
        startDate: startDate,
        endDate: endDate
    }
    const paramsStr = objectToQueryString(params);
    const url = `${baseUrl}/GetIncidentsForTypeAndDate?${paramsStr}`;
    return axios.get(url, { crossDomain: true })
        .then(response => {
            return response;
        });
}

