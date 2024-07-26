import React from "react";
import { Form, FormProps } from "reactstrap";

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
