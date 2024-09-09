import React from "react";
import { Progress } from "reactstrap";
import { HipTooltip } from "./HipTooltip";

export interface HipProgressProps {
  id: string;
  tooltip?: React.ReactNode;
  bar?: boolean;
  label?: string;
  max?: number;
  shouldFill?: boolean;
  striped?: boolean;
  animated?: boolean;
  value?: number;
  color?: string;
}

const HipProgress: React.FC<HipProgressProps> = ({
  id,
  tooltip,
  bar = true,
  label,
  max,
  shouldFill,
  striped,
  animated,
  value,
  color,
  ...rest
}) => {
  const target = "hip-progress-" + id;
  return (
    <>
      <Progress
        id={target}
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
      {!!tooltip && (
        <HipTooltip target={target} placement="top">
          {tooltip}
        </HipTooltip>
      )}
    </>
  );
};

export default HipProgress;
