import React from "react";
import { Col, FormGroup, FormGroupProps } from "reactstrap";

interface HipFormGroupProps extends FormGroupProps {
  className?: string;
  children: React.ReactNode;
}

/**
 * HipFormGroup component that wraps reactstrap FormGroup with additional classNames.
 */
export const HipFormGroup: React.FC<HipFormGroupProps> = ({
  className,
  children,
  ...rest
}) => {
  return (
    <Col>
      <FormGroup className={`hip-form-group ${className ?? ""}`} {...rest}>
        {children}
      </FormGroup>
    </Col>
  );
};
