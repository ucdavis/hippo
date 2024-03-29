import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { AddToGroupModel, GroupModel, RequestModel } from "../../types";
import { GroupInfo } from "../Group/GroupInfo";
import { GroupLookup } from "../Group/GroupLookup";
import { CardColumns } from "reactstrap";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { authenticatedFetch } from "../../util/api";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import SshKeyInput from "../../Shared/SshKeyInput";

export const AccountInfo = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [context, setContext] = useContext(AppContext);
  const { cluster: clusterName } = useParams();
  const account = context.accounts.find((a) => a.cluster === clusterName);
  const cluster = context.clusters.find((c) => c.name === clusterName);
  const navigate = useNavigate();

  const currentGroups = useMemo(() => account?.memberOfGroups ?? [], [account]);
  const currentOpenRequests = useMemo(
    () => context.openRequests.filter((r) => r.cluster === clusterName),
    [context.openRequests, clusterName],
  );

  const [groups, setGroups] = useState<GroupModel[]>([]);
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(
        `/api/${clusterName}/group/groups`,
      );

      if (response.ok) {
        setGroups(
          ((await response.json()) as GroupModel[]).filter(
            (g) =>
              !currentGroups.some((cg) => cg.id === g.id) &&
              !currentOpenRequests.some((r) => r.groupModel.id === g.id),
          ),
        );
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [clusterName, currentOpenRequests, currentGroups]);

  const [getGroupConfirmation] = useConfirmationDialog<AddToGroupModel>(
    {
      title: "Request Access to Group",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <GroupLookup
                  setSelection={(selection) =>
                    setReturn((model) => ({ ...model, groupId: selection?.id }))
                  }
                  options={groups}
                />
              </div>
              <div className="form-group">
                <label className="form-label">
                  Who is your supervising PI?
                </label>
                <input
                  className="form-control"
                  id="supervisingPI"
                  placeholder="Supervising PI"
                  onChange={(e) =>
                    setReturn((model) => ({
                      ...model,
                      supervisingPI: e.target.value,
                    }))
                  }
                ></input>
                <p className="form-helper">
                  Some clusters may require additional clarification on who your
                  supervising PI will be for this group. If you are unsure,
                  please ask your sponsor.
                </p>
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => returnValue !== undefined,
    },
    [groups],
  );

  const [getSshKeyConfirmation] = useConfirmationDialog<string>(
    {
      title: "Update your SSH Key",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <label className="form-label">
                  Please paste your public SSH key.
                </label>
                <SshKeyInput onChange={setReturn} />
                <p className="form-helper">
                  To generate a ssh key pair please see this link:{" "}
                  <a
                    href="https://hpc.ucdavis.edu/faq#ssh-key"
                    target={"blank"}
                  >
                    https://hpc.ucdavis.edu/faq#ssh-key
                  </a>
                </p>
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => notEmptyOrFalsey(returnValue),
    },
    [],
  );

  const handleRequestAccess = useCallback(async () => {
    const [confirmed, addToGroupModel] = await getGroupConfirmation();
    if (confirmed) {
      const request = authenticatedFetch(
        `/api/${clusterName}/group/requestaccess/`,
        {
          method: "POST",
          body: JSON.stringify(addToGroupModel),
        },
      );

      setNotification(request, "Sending Request", "Request Sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
      if (response.ok) {
        const newRequest = (await response.json()) as RequestModel;
        setContext((c) => ({
          ...c,
          openRequests: [...c.openRequests, newRequest],
        }));
        setGroups((g) => g.filter((g) => g.id !== addToGroupModel.groupId));
      }
    }
  }, [clusterName, getGroupConfirmation, setNotification, setContext]);

  const handleUpdateSshKey = useCallback(async () => {
    const [confirmed, sshKey] = await getSshKeyConfirmation();

    if (confirmed) {
      const request = authenticatedFetch(
        `/api/${clusterName}/account/updatessh`,
        {
          method: "POST",
          body: JSON.stringify({
            accountId: account?.id,
            sshKey: sshKey,
          }),
        },
      );

      setNotification(request, "Sending Request", "Request Sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
    }
  }, [account?.id, clusterName, getSshKeyConfirmation, setNotification]);

  useEffect(() => {
    if (!account) {
      const request = currentOpenRequests.find(
        (r) => r.cluster === clusterName && r.action === "CreateAccount",
      );
      if (request) {
        navigate(`/${clusterName}/accountstatus`);
      }
      navigate(`/${clusterName}/create`);
    }
  }, [account, clusterName, currentOpenRequests, navigate]);

  return (
    <>
      <div className="row justify-content-center">
        <div className="col-md-8">
          {currentGroups.length ? (
            <p>
              Welcome {context.user.detail.firstName}. Your account is
              registered with the following group(s):
            </p>
          ) : (
            <p>
              Welcome {context.user.detail.firstName}. Your account is not
              associated with any groups.
            </p>
          )}

          <CardColumns>
            {currentGroups.map((g, i) => (
              <div className="group-card-admin" key={i}>
                <GroupInfo group={g} />
              </div>
            ))}
          </CardColumns>

          {Boolean(currentOpenRequests.length) && (
            <>
              <p>You have pending requests for the following group(s):</p>

              <CardColumns>
                {currentOpenRequests.map((r, i) => (
                  <div className="group-card-admin" key={i}>
                    <GroupInfo group={r.groupModel} />
                  </div>
                ))}
              </CardColumns>
            </>
          )}
          <br />

          <div>
            <button
              disabled={notification.pending || groups.length === 0}
              onClick={() => handleRequestAccess()}
              className="btn btn-primary btn-sm"
            >
              Request Access to Another Group
            </button>{" "}
            {cluster.enableUserSshKey && (
              <button
                disabled={notification.pending || groups.length === 0}
                onClick={() => handleUpdateSshKey()}
                className="btn btn-primary btn-sm"
              >
                Update SSH Key
              </button>
            )}
          </div>
        </div>
      </div>
    </>
  );
};
