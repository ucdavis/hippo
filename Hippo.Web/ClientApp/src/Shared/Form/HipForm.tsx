import React from "react";
import { Form } from "reactstrap";

interface HipFormProps extends React.FormHTMLAttributes<HTMLFormElement> {}

/**
 * A form component that wraps the Form component from reactstrap with appropriate classNames
 */
export const HipForm: React.FC<HipFormProps> = ({
  className,
  children,
  ...rest
}) => {
  return (
    <Form className={`hip-form mb-3 ${className ?? ""}`} {...rest}>
      {children}
    </Form>
  );
};
