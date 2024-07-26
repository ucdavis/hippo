import React from "react";
import { Form, FormGroup, FormGroupProps, FormProps } from "reactstrap";

interface HipFormProps extends FormProps {
  wrap?: boolean;
}

export const HipForm: React.FC<HipFormProps> = ({
  wrap,
  className,
  children,
  ...rest
}) => {
  return (
    <Form
      className={`hip-form mb-3 ${className}${wrap ? " wrap" : ""}`}
      {...rest}
    >
      {children}
    </Form>
  );
};

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
