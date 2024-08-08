import React from "react";
import { FormGroup, FormGroupProps } from "reactstrap";

interface HipFormGroupProps extends FormGroupProps {
  size?: "sm" | "md" | "lg";
  colSize?: number;
  className?: string;
  children: React.ReactNode;
}

/**
 * HipFormGroup component that wraps reactstrap FormGroup with additional classNames.
 */
export const HipFormGroup: React.FC<HipFormGroupProps> = ({
  size = "md",
  colSize,
  className,
  children,
  ...rest
}) => {
  const columnSize = colSize ?? size === "sm" ? 3 : size === "md" ? 6 : 12;

  return (
    <div className={`col-12 col-md-${columnSize}`}>
      <FormGroup className={`hip-form-group ${className ?? ""}`} {...rest}>
        {children}
      </FormGroup>
    </div>
  );
};
