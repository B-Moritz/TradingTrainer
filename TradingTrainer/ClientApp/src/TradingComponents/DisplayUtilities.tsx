type DateTimeFormatProps = {
    rawDateTimeFormat : string
}

function DateTimeFormat(props : DateTimeFormatProps) : JSX.Element {
    const dateTimeFormat = (rawFormat : string) : string => {
        // This method convert the date time string to a more concise format
        const regex = /([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9])/;
        let matchResult = regex.exec(rawFormat);
        if (matchResult === null) {
            return rawFormat;
        }
        const tradingTimeFormat = `${matchResult[1]}.${matchResult[2]}.${matchResult[3]}   ${matchResult[4]}:${matchResult[5]}:${matchResult[6]}`
        return tradingTimeFormat;
    }

    return(
        <span>
            {dateTimeFormat(props.rawDateTimeFormat)}
        </span>
    );
}
export {DateTimeFormat};