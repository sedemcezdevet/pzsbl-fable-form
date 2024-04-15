import AsyncSelect from 'react-select/async'

export function SelectSearch({
    disabled,
    placeholder,
    value,
    onChange,
    onBlur,
    loadOptions,
    getOptionValue,
    getOptionLabel,
}) {
    return (
        <AsyncSelect
            classNamePrefix="select-search"
            cacheOptions
            defaultOptions
            isDisabled={disabled}
            placeholder={placeholder}
            loadOptions={loadOptions}
            getOptionValue={getOptionValue}
            getOptionLabel={getOptionLabel}
            value={value}
            onChange={onChange}
            onBlur={onBlur}
            isClearable={true}
        />
    )
}
