import React from "react";
import { Progress, ProgressProps } from "reactstrap";

interface HipProgressProps extends ProgressProps {
  bar?: boolean;
  label?: string;
  max?: number;
  shouldFill?: boolean;
  striped?: boolean;
  animated?: boolean;
  tooltip?: React.ReactNode;
}

const HipProgress: React.FC<HipProgressProps> = ({
  bar,
  label,
  max,
  shouldFill,
  striped,
  animated,
  value,
  color,
  ...rest
}) => {
  return (
    <Progress
      bar={bar}
      max={max}
      value={value ?? 1}
      color={
        color ?? (shouldFill ? "primary" : striped ? "secondary" : "tertiary")
      }
      striped={striped} // but show stripes for next step status
      animated={animated}
      {...rest}
    >
      {label}
    </Progress>
  );
};

export default HipProgress;
