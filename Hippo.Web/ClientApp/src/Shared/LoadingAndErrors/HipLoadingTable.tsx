import React from "react";
import { faHippo } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Table, TableProps } from "reactstrap";
import HipTableActionButton from "../Table/HipTableActionButton";
import HipLoading from "./HipLoading";

interface HipLoadingTableProps extends TableProps {
  columns?: number;
  rows?: number;
}

const HipLoadingTable: React.FC<HipLoadingTableProps> = ({
  columns = 7,
  rows = 10,
  ...deferred
}) => {
  return (
    <>
      <HipTableActionButton>
        <HipLoading />
      </HipTableActionButton>
      <Table striped={true} bordered={true} {...deferred}>
        <thead>
          <tr>
            {Array.from({ length: columns }).map((_, i) => (
              <th key={i}>
                {i === -1 ? (
                  <div className="skeleton">
                    <FontAwesomeIcon icon={faHippo} size="2x" bounce />
                  </div>
                ) : (
                  <div className="skeleton skeleton-header"></div>
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {Array.from({ length: rows }).map((_, i) => (
            <tr key={i}>
              {Array.from({ length: columns }).map((_, j) => (
                <td key={j}>
                  <div className="skeleton skeleton-cell"></div>
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </Table>
    </>
  );
};

export default HipLoadingTable;
