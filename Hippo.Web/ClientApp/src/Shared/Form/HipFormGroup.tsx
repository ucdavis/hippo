import React from "react";
import { FormGroupProps, FormGroup } from "reactstrap";

interface HipFormGroupProps extends FormGroupProps {
  noWrap?: boolean;
  readOnly?: boolean;
}

export const HipFormGroup: React.FC<HipFormGroupProps> = ({
  noWrap,
  readOnly,
  className,
  children,
  ...rest
}) => {
  return (
    <FormGroup
      className={`hip-form-group ${className}${noWrap ? " no-wrap" : ""}${readOnly ? " read-only" : ""}`}
      {...rest}
    >
      {children}
    </FormGroup>
  );
};
