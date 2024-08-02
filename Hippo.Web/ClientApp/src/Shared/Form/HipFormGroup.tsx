import React from "react";
import { Col, FormGroup, FormGroupProps } from "reactstrap";

interface HipFormGroupProps extends FormGroupProps {
  size: "sm" | "md" | "lg";
  readOnly?: boolean;
  className?: string;
  children: React.ReactNode;
}

/**
 * HipFormGroup component that wraps reactstrap FormGroup with additional classNames.
 */
export const HipFormGroup: React.FC<HipFormGroupProps> = ({
  size,
  readOnly,
  className,
  children,
  ...rest
}) => {
  return (
    <Col>
      <FormGroup
        className={`hip-form-group ${className ?? ""} hip-size-${size}`}
        {...rest}
      >
        {children}
      </FormGroup>
    </Col>
  );
};
