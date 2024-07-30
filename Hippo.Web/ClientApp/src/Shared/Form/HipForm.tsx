import React from "react";
import { Form } from "reactstrap";

interface HipFormProps extends React.FormHTMLAttributes<HTMLFormElement> {
  wrap?: boolean;
}

/**
 * A form component that wraps the Form component from reactstrap with appropriate classNames
 * @param wrap - Whether or not the form should flex-wrap its children
 */
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
