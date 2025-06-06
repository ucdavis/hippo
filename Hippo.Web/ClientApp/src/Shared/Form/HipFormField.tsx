import React from "react";
import { FormFeedback, FormText, Input, Label } from "reactstrap";
import HipInputGroup from "./HipInputGroup";
import { HipFormFieldProps } from "./formTypes";
import { HipFormGroup } from "./HipFormGroup";
import { Path, RegisterOptions } from "react-hook-form";

/**
 * HipFormField component that combines react-hook-form register with reactstrap input.
 *
 * To display only as text, set `readOnly` to true. This will still register the field so it is included in the form state.
 * It will only display the value, not any children passed to it (like with `<select>` and `<options>`).
 *
 * 'hip-form-group' is added to the FormGroup
 */
const HipFormField = <T extends Record<string, any>>({
  register,
  getValues, // optional, but required if hideIfEmpty is true
  hideIfEmpty = false,
  error,
  valid,
  feedback,
  feedbackType = "text",
  type = "text",
  name,
  label,
  hideLabel = false,
  inputPrepend,
  inputAppend,
  required = false,
  maxLength,
  minLength,
  max,
  min,
  readOnly = false,
  autoComplete,
  children,
  size,
  colSize,
  disabled, // select out disabled and don't pass it to register or it will set the value to undefined
  valueAsNumber,
  ...options
}: HipFormFieldProps<T>) => {
  const { ref, ...rest } = register(name, {
    required: {
      value: required,
      message: `${label} is required`,
    },
    maxLength: {
      value: maxLength,
      message: `${label} must be less than ${maxLength} characters`,
    },
    minLength: {
      value: minLength,
      message: `${label} must be at least ${minLength} characters`,
    },
    max: {
      value: max,
      message: `${label} must be less than ${max}`,
    },
    min: {
      value: min,
      message: `${label} must be greater than ${min}`,
    },
    ...options,
  } as RegisterOptions<T, string & Path<T>>);

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    // prevent entering nonnumeric characters if valueAsNumber is true
    if (
      valueAsNumber &&
      !/[\d.-]/.test(event.key) &&
      event.key !== "Backspace"
    ) {
      event.preventDefault();
    }
  };

  const handlePaste = (event: React.ClipboardEvent<HTMLInputElement>) => {
    // prevent pasting nonnumeric characters if valueAsNumber is true
    if (valueAsNumber) {
      const pasteData = event.clipboardData.getData("text");
      if (!/^[\d.-]+$/.test(pasteData)) {
        event.preventDefault();
      }
    }
  };

  const value = getValues ? getValues(name) : null;
  if (readOnly && disabled && hideIfEmpty && !value) {
    return null;
  }
  return (
    <HipFormGroup size={size} colSize={colSize}>
      {label && !hideLabel && (
        <Label for={`field-${name}`}>
          {label}
          {required && !readOnly && <span> *</span>}
        </Label>
      )}
      <HipInputGroup
        prepend={inputPrepend}
        append={inputAppend}
        readOnly={readOnly}
      >
        <Input
          className={`hip-form-field`}
          innerRef={ref}
          id={`field-${name}`}
          name={autoComplete ? name : `field-${name}`} // less likely to autofill
          type={readOnly ? "text" : type}
          tag={type === "textarea" ? "textarea" : undefined}
          invalid={!!error}
          valid={valid}
          readOnly={readOnly}
          plaintext={readOnly}
          disabled={disabled}
          autoComplete="none"
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          {...rest}
        >
          {!readOnly ? children : null}
        </Input>
        {feedback &&
          (feedbackType === "tooltip" ? (
            <FormFeedback valid={true} tooltip={true}>
              {feedback}
            </FormFeedback>
          ) : (
            <FormText>{feedback}</FormText>
          ))}
        {!!error && <FormFeedback valid={false}>{error.message}</FormFeedback>}
      </HipInputGroup>
    </HipFormGroup>
  );
};

export default HipFormField;
