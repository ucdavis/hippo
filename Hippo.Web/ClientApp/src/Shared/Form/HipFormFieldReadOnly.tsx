import React from "react";
import { Label } from "reactstrap";
import { HipFormGroup } from "./HipFormGroup";
import HipInputGroup from "./HipInputGroup";

type HipFormFieldReadonlyProps = {
  readOnly: true;
  label: string;
  name: string;
  inputPrepend?: React.ReactNode;
  inputAppend?: React.ReactNode;
  value: string;
  type?: string;
};

/**
 * HipFieldReadOnly component that displays a readonly field, **outside of a Form**.
 *
 * 'hip-form-field' and 'form-control-plaintext' are added to the input
 */
const HipFieldReadOnly: React.FC<HipFormFieldReadonlyProps> = ({
  inputPrepend,
  inputAppend,
  name,
  label,
  value,
}) => {
  return (
    <HipFormGroup>
      {label && <Label for={`field-${name}`}>{label}</Label>}
      <HipInputGroup readOnly prepend={inputPrepend} append={inputAppend}>
        <span className="hip-form-field form-control-plaintext">{value}</span>
      </HipInputGroup>
    </HipFormGroup>
  );
};

export default HipFieldReadOnly;
