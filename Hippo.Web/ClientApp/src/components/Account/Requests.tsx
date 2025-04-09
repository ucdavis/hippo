import { useEffect, useState, useMemo, useCallback, useContext } from "react";
import { RequestModel } from "../../types";
import { RejectRequest } from "../../Shared/RejectRequest";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";
import { HipTable } from "../../Shared/Table/HipTable";
import { SplitCamelCase, getGroupModelString } from "../../util/StringHelpers";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { isAccountRequest } from "../../util/TypeChecks";
import { createColumnHelper } from "@tanstack/react-table";
import HipButton from "../../Shared/HipComponents/HipButton";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import { getGroupModelFromRequest } from "../../Shared/requestUtils";
import AppContext from "../../Shared/AppContext";

export const Requests = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each request

  const [requests, setRequests] = useState<RequestModel[]>();
  const [requestApproving, setRequestApproving] = useState<number>();
  const [notification, setNotification] = usePromiseNotification();

  const { cluster: clusterName } = useParams();
  const [context, _] = useContext(AppContext);
  const cluster = context.clusters.find((c) => c.name === clusterName);

  useEffect(() => {
    const fetchRequests = async () => {
      const response = await authenticatedFetch(
        `/api/${clusterName}/request/pending`,
      );

      if (response.ok) {
        setRequests((await response.json()) as RequestModel[]);
      }
    };

    fetchRequests();
  }, [clusterName]);

  const handleApprove = useCallback(
    async (request: RequestModel) => {
      setRequestApproving(request.id);

      const req = authenticatedFetch(
        `/api/${clusterName}/request/approve/${request.id}`,
        {
          method: "POST",
        },
      );

      setNotification(
        req,
        "Approving",
        "Request Approved. Please allow 2 to 3 hours for changes to take place.",
      );

      const response = await req;
      if (response.ok) {
        setRequestApproving(undefined);

        // remove the request from the list
        setRequests(requests?.filter((a) => a.id !== request.id));
      }
    },
    [requests, clusterName, setNotification],
  );

  const handleReject = useCallback(
    async (request: RequestModel) => {
      // remove the request from the list
      setRequests(requests?.filter((a) => a.id !== request.id));
    },
    [requests],
  );

  const columnHelper = createColumnHelper<RequestModel>();

  const columns = [];
  columns.push(
    columnHelper.accessor((request) => SplitCamelCase(request.action), {
      header: "Request",
      meta: {
        filterVariant: "select",
      },
    }),
  );
  columns.push(
    columnHelper.accessor("requesterName", {
      header: "Name",
      id: "Name", // id required for only this column for some reason
    }),
  );
  columns.push(
    columnHelper.accessor("requesterEmail", {
      header: "Email",
    }),
  );
  columns.push(
    columnHelper.accessor(
      (row) => getGroupModelString(getGroupModelFromRequest(row)),
      {
        header: "Group",
        cell: (props) => {
          const groupModel = getGroupModelFromRequest(props.row.original);
          return (
            <GroupNameWithTooltip group={groupModel} showDisplayName={false} />
          );
        },
        meta: {
          exportFn: (request) => request.groupModel.displayName,
        },
      },
    ),
  );
  columns.push(
    columnHelper.accessor(
      (request) =>
        isAccountRequest(request) ? request.data.supervisingPI : "",
      {
        header: "Supervising PI",
      },
    ),
  );
  columns.push(
    columnHelper.accessor(
      (request) =>
        isAccountRequest(request) ? request.data.accessTypes.join(", ") : "",
      {
        header: "Access Types",
        meta: {
          filterVariant: "select",
        },
      },
    ),
  );
  columns.push(
    columnHelper.accessor(
      (request) =>
        isAccountRequest(request) ? request.data.accessTypes.join(", ") : "",
      {
        header: "Access Types",
        meta: {
          filterVariant: "select",
        },
      },
    ),
  );
  // only show this column if cluster has an AUP
  if (cluster.acceptableUsePolicyUrl && cluster.acceptableUsePolicyUpdatedOn) {
    columns.push(
      columnHelper.accessor(
        (request) =>
          isAccountRequest(request)
            ? request.data.acceptableUsePolicyAgreedOn
            : "n/a",
        {
          header: "AUP Agreed On",
          cell: (props) => {
            const value = props.getValue();
            if (value === "n/a") return value;
            return new Date(value).toLocaleDateString();
          },
        },
      ),
    );
  }
  columns.push(
    columnHelper.display({
      id: "actions",
      header: "Action",
      cell: (props) => (
        <>
          <HipButton
            id="approveButton"
            disabled={notification.pending}
            onClick={() => handleApprove(props.row.original)}
          >
            {requestApproving === props.row.original.id
              ? "Approving..."
              : "Approve"}
          </HipButton>
          {requestApproving !== props.row.original.id && (
            <RejectRequest
              request={props.row.original}
              removeAccount={() => handleReject(props.row.original)}
              updateUrl={`/api/${clusterName}/request/reject/`}
              disabled={notification.pending}
            ></RejectRequest>
          )}
        </>
      ),
    }),
  );

  const accountsData = useMemo(() => requests ?? [], [requests]);

  const Title = (
    <HipTitle title="Pending Approvals" subtitle="Administration" />
  );

  if (requests === undefined) {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <p>There are {requests.length} request(s) awaiting approval</p>
          <HipTable
            columns={columns}
            data={accountsData}
            initialState={{
              sorting: [
                { id: "Request", desc: false },
                { id: "Name", desc: false },
              ],
            }}
          />
        </HipBody>
      </HipMainWrapper>
    );
  }
};
