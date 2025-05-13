import axios from "axios";
import { objectToQueryString } from "../helpers/webrequest";


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
    const url = `${import.meta.env.VITE_API_URL}/PoliceIncident/GetIncidentsForTypeAndDate?${paramsStr}`;
    return axios.get(url, { crossDomain: true })
        .then(response => {
            return response;
        });
}

