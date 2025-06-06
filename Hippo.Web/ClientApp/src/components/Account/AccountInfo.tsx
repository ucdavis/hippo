import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import {
  AddToGroupModel,
  GroupRequestDataModel,
  GroupModel,
  RequestModel,
  User,
} from "../../types";
import { GroupInfo } from "../Group/GroupInfo";
import { GroupLookup } from "../Group/GroupLookup";
import { CardColumns } from "reactstrap";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import SshKeyInput from "../../Shared/SshKeyInput";
import GroupDetails from "../Group/GroupDetails";
import ObjectTree from "../../Shared/ObjectTree";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipButton from "../../Shared/HipComponents/HipButton";
import { getGroupModelFromRequest } from "../../Shared/requestUtils";
import { SearchPerson } from "../../Shared/SearchPerson";

export const AccountInfo = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [context, setContext] = useContext(AppContext);
  const { cluster: clusterName } = useParams();
  const account = context.accounts.find((a) => a.cluster === clusterName);
  const cluster = context.clusters.find((c) => c.name === clusterName);
  const navigate = useNavigate();
  const userGroupName = context.user.detail.kerberos + "grp";
  const [supervisingPI, setSupervisingPI] = useState<User>();

  const memberOfGroups = useMemo(
    () => account?.memberOfGroups ?? [],
    [account],
  );
  const adminOfGroups = useMemo(() => account?.adminOfGroups ?? [], [account]);
  const currentOpenRequests = useMemo(
    () => context.openRequests.filter((r) => r.cluster === clusterName),
    [context.openRequests, clusterName],
  );

  const [availableGroups, setAvailableGroups] = useState<GroupModel[]>([]);
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(
        `/api/${clusterName}/group/groups`,
      );

      if (response.ok) {
        setAvailableGroups(
          ((await response.json()) as GroupModel[]).filter(
            (g) =>
              !memberOfGroups.some((cg) => cg.id === g.id) &&
              !adminOfGroups.some((cg) => cg.id === g.id) &&
              !currentOpenRequests.some((r) => r.groupModel?.id === g.id),
          ),
        );
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [adminOfGroups, clusterName, currentOpenRequests, memberOfGroups]);

  const [getGroupAccessConfirmation] = useConfirmationDialog<AddToGroupModel>(
    {
      title: "Request Access to Group",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-12">
              <div className="form-group">
                <GroupLookup
                  setSelection={(selection) =>
                    setReturn((model) => ({ ...model, groupId: selection?.id }))
                  }
                  options={availableGroups}
                />
              </div>
              <div className="form-group">
                <label className="form-label">
                  Who is your supervising PI?
                </label>
                <SearchPerson
                  user={supervisingPI}
                  onChange={(user) => {
                    setReturn((model) => ({
                      ...model,
                      supervisingPI: user?.name,
                      supervisingPIIamId: user?.iam,
                    }));
                    setSupervisingPI(user);
                  }}
                />                
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
    [availableGroups, supervisingPI],
  );

  const [getGroupCreationConfirmation] =
    useConfirmationDialog<GroupRequestDataModel>(
      {
        title: "Request Group Creation",
        message: (setReturn) => {
          return (
            <div className="row justify-content-center">
              <div className="col-md-12">
                <div className="form-group">
                  <label className="form-label">Display Name</label>
                  <input
                    className="form-control"
                    id="displayName"
                    placeholder="Enter Display Name"
                    onChange={(e) =>
                      setReturn((_) => ({
                        name: userGroupName,
                        displayName: e.target.value,
                      }))
                    }
                  ></input>
                  <p className="form-helper">
                    If approved, your group will be named <b>{userGroupName}</b>
                    .
                  </p>
                </div>
              </div>
            </div>
          );
        },
        canConfirm: (returnValue) =>
          (returnValue?.displayName ?? "").length >= 3,
      },
      [availableGroups],
    );

  const [getSshKeyConfirmation] = useConfirmationDialog<string>(
    {
      title: "Update your SSH Key",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-12">
              <div className="form-group">
                <label className="form-label">
                  Please paste your public SSH key.
                </label>
                <SshKeyInput onChange={setReturn} />
                <p className="form-helper">
                  To generate a ssh key pair please see this link:{" "}
                  <a
                    href="https://hpc.ucdavis.edu/faq/access-to-hpc#ssh-key"
                    target={"blank"}
                  >
                    https://hpc.ucdavis.edu/faq/access-to-hpc#ssh-key
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

  const handleRequestGroupAccess = useCallback(async () => {
    const [confirmed, addToGroupModel] = await getGroupAccessConfirmation();
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
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
      if (response.ok) {
        const requestModel = (await response.json()) as RequestModel;

        setContext((c) => ({
          ...c,
          openRequests: [...c.openRequests, requestModel],
        }));
        setAvailableGroups((g) =>
          g.filter((g) => g.id !== addToGroupModel.groupId),
        );
      }
    }
  }, [clusterName, getGroupAccessConfirmation, setNotification, setContext]);

  const handleRequestGroupCreation = useCallback(async () => {
    const [confirmed, createGroupModel] = await getGroupCreationConfirmation();
    if (confirmed) {
      const request = authenticatedFetch(
        `/api/${clusterName}/group/requestcreation/`,
        {
          method: "POST",
          body: JSON.stringify(createGroupModel),
        },
      );

      setNotification(request, "Sending Request", "Request Sent", async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
      if (response.ok) {
        const requestModel = (await response.json()) as RequestModel;

        setContext((c) => ({
          ...c,
          openRequests: [...c.openRequests, requestModel],
        }));
      }
    }
  }, [getGroupCreationConfirmation, clusterName, setNotification, setContext]);

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
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
    }
  }, [account?.id, clusterName, getSshKeyConfirmation, setNotification]);

  const [showDetails] = useConfirmationDialog(
    {
      title: "Account Details",
      message: () => {
        return <ObjectTree obj={account} />;
      },
      buttons: ["OK"],
    },
    [account],
  );

  const handleViewDetails = async () => {
    await showDetails();
  };

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

  const [showingGroup, setShowingGroup] = useState<GroupModel>();

  const [showGroupDetails] = useConfirmationDialog(
    {
      title: "Group Details",
      message: () => {
        return <GroupDetails group={showingGroup} />;
      },
      buttons: ["OK"],
    },
    [showingGroup],
  );

  const handleShowGroup = (group: GroupModel) => {
    setShowingGroup(group);
    showGroupDetails();
  };

  const handleNavigateToGroupMembers = (group: GroupModel) => {
    navigate(`/${clusterName}/group/${group.id}`);
  };

  return (
    <HipMainWrapper>
      <HipTitle title={`Welcome ${context.user.detail.firstName}`} />
      <HipBody>
        {!!memberOfGroups.length && (
          <>
            <p>Your account is registered with the following group(s):</p>
            <CardColumns>
              {memberOfGroups.map((g, i) => (
                <div className="group-card-admin" key={i}>
                  <GroupInfo group={g} showDetails={() => handleShowGroup(g)} />
                </div>
              ))}
            </CardColumns>
            <br />
          </>
        )}

        {!!adminOfGroups.length && (
          <>
            <p>You are an admin for the following group(s):</p>
            <CardColumns>
              {adminOfGroups.map((g, i) => (
                <div className="group-card-admin" key={i}>
                  <GroupInfo
                    group={g}
                    showDetails={() => handleShowGroup(g)}
                    navigateToGroupMembers={() =>
                      handleNavigateToGroupMembers(g)
                    }
                  />
                </div>
              ))}
            </CardColumns>
            <br />
          </>
        )}

        {Boolean(currentOpenRequests.length) && (
          <>
            <p>You have pending requests for the following group(s):</p>

            <CardColumns>
              {currentOpenRequests.map((r, i) => {
                const groupModel = getGroupModelFromRequest(r);
                return (
                  <div className="group-card-admin" key={i}>
                    <GroupInfo
                      group={groupModel}
                      showDetails={() => handleShowGroup(groupModel)}
                    />
                  </div>
                );
              })}
            </CardColumns>
            <br />
          </>
        )}
        <div>
          <HipButton onClick={() => handleViewDetails()} size="sm">
            View Account Details
          </HipButton>{" "}
          <HipButton
            disabled={notification.pending || availableGroups.length === 0}
            onClick={() => handleRequestGroupAccess()}
            size="sm"
          >
            Request Access to Another Group
          </HipButton>{" "}
          {context.featureFlags.createGroup && !adminOfGroups.some((g) => g.name === userGroupName) &&
            !currentOpenRequests.some(
              (r) =>
                r.action === "CreateGroup" && r.data.name === userGroupName,
            ) && (
              <HipButton
                disabled={notification.pending}
                onClick={() => handleRequestGroupCreation()}
                size="sm"
              >
                Request Group Creation
              </HipButton>
            )}{" "}
          {cluster.accessTypes.includes("SshKey") && (
            <HipButton
              disabled={notification.pending || availableGroups.length === 0}
              onClick={() => handleUpdateSshKey()}
              size="sm"
            >
              Update SSH Key
            </HipButton>
          )}
        </div>
      </HipBody>
    </HipMainWrapper>
  );
};
