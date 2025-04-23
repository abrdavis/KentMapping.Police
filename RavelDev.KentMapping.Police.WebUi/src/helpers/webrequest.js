export {objectToQueryString};

function objectToQueryString(obj){
    return Object.keys(obj)
        .map(key => {
            let mappedValue = '';
            if (obj[key] == null)
                mappedValue = `${encodeURIComponent(key)}=`
            else
                mappedValue = `${encodeURIComponent(key)}=${encodeURIComponent(obj[key])}`
            return mappedValue;
        }
            )
        .join('&');
}