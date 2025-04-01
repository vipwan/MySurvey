import React, { useState, useEffect, useRef } from 'react';
import { Table, Button, Space, Tag, Modal, message, Typography, Tooltip, Input, Dropdown, Tabs, Divider, App, theme } from 'antd';
import {
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    EyeOutlined,
    CopyOutlined,
    QuestionCircleOutlined,
    ShareAltOutlined,
    FileTextOutlined,
    CheckOutlined,
    CloseOutlined,
    MoreOutlined,
    LinkOutlined,
    QrcodeOutlined,
    DownloadOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { surveyApi } from '../../services/api';
import { QRCodeSVG } from 'qrcode.react';

const { Title } = Typography;
const { useToken } = theme;

const SurveyList = () => {
    // 获取主题token，用于颜色统一
    const { token } = useToken();

    // 添加引用，用于解决findDOMNode警告
    const tableRef = useRef(null);
    const modalRef = useRef(null);

    const [surveys, setSurveys] = useState([]);
    const [loading, setLoading] = useState(false);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 10,
    });
    const [shareModalVisible, setShareModalVisible] = useState(false);
    const [currentSurvey, setCurrentSurvey] = useState(null);
    const [shareLink, setShareLink] = useState('');
    const [activeTab, setActiveTab] = useState('link');
    const navigate = useNavigate();
    const { modal } = App.useApp(); // 使用 App.useApp() 获取 modal 实例

    // 按钮样式定义
    const actionBtnStyle = {
        borderRadius: '4px',
        display: 'flex',
        alignItems: 'center',
        transition: 'all 0.3s',
    };

    const actionStyles = {
        question: {
            color: token.colorInfo,
            '&:hover': {
                color: token.colorInfoHover,
                background: token.colorInfoBg,
            }
        },
        preview: {
            color: token.colorPrimary,
            '&:hover': {
                color: token.colorPrimaryHover,
                background: token.colorPrimaryBg,
            }
        },
        edit: {
            color: token.colorWarning,
            '&:hover': {
                color: token.colorWarningHover,
                background: token.colorWarningBg,
            }
        },
        share: {
            color: token.colorSuccess,
            '&:hover': {
                color: token.colorSuccessHover,
                background: token.colorSuccessBg,
            }
        },
        export: {
            color: token.colorPrimary,
            '&:hover': {
                color: token.colorPrimaryHover,
                background: token.colorPrimaryBg,
            }
        },
        copy: {
            color: token.colorInfo,
            '&:hover': {
                color: token.colorInfoHover,
                background: token.colorInfoBg,
            }
        },
        delete: {
            color: token.colorError,
            '&:hover': {
                color: token.colorErrorHover,
                background: token.colorErrorBg,
            }
        }
    };

    const fetchSurveys = async () => {
        try {
            setLoading(true);
            const response = await surveyApi.getSurveys({
                status: 0, // 所有状态
                pageSize: pagination.pageSize,
                pageNumber: pagination.current,
            });
            // 修改这里，直接使用 response.data
            setSurveys(response.data);
            setPagination({
                ...pagination,
                total: response.data.length,
            });
        } catch (error) {
            message.error('获取问卷列表失败');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSurveys();
    }, [pagination.current, pagination.pageSize]);

    const handleDelete = async (id) => {
        // 使用 modal.confirm 替代 Modal.confirm
        modal.confirm({
            title: '确认删除',
            content: '确定要删除这个问卷吗？此操作不可恢复。',
            okButtonProps: { danger: true },
            onOk: async () => {
                try {
                    await surveyApi.deleteSurvey(id);
                    message.success('删除成功');
                    fetchSurveys();
                } catch (error) {
                    message.error('删除失败');
                }
            },
        });
    };

    const handleCopy = async (id) => {
        try {
            await surveyApi.copySurvey(id);
            message.success('复制成功');
            fetchSurveys();
        } catch (error) {
            message.error('复制失败');
        }
    };

    // 处理状态变更
    const handleStatusChange = async (id, newStatus) => {
        try {
            if (newStatus === 0) { // 设置为草稿
                // 先复制一份当前问卷
                const result = await surveyApi.copySurvey(id);
                message.success('问卷已复制为草稿状态');
                fetchSurveys();
            } else if (newStatus === 1) { // 发布问卷
                await surveyApi.publishSurvey(id);
                message.success('问卷已发布');
                fetchSurveys();
            } else if (newStatus === 2) { // 结束问卷
                await surveyApi.endSurvey(id);
                message.success('问卷已结束');
                fetchSurveys();
            }
        } catch (error) {
            message.error(`状态变更失败: ${error.response?.data || error.message}`);
        }
    };

    // 处理分享
    const handleShare = (record) => {
        setCurrentSurvey(record);
        const link = `${window.location.origin}/anonymous-survey/${record.id}`;
        setShareLink(link);
        setShareModalVisible(true);
        setActiveTab('link');
    };

    // 复制分享链接
    const copyShareLink = () => {
        navigator.clipboard.writeText(shareLink)
            .then(() => {
                message.success('链接已复制到剪贴板');
            })
            .catch(() => {
                message.error('复制失败，请手动复制');
            });
    };

    // 处理导出
    const handleExport = async (id) => {
        try {
            const response = await surveyApi.exportAnswers(id);
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `survey_answers_${id}.xlsx`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
            message.success('导出成功');
        } catch (error) {
            message.error('导出失败');
        }
    };

    const columns = [
        {
            title: '标题',
            dataIndex: 'title',
            key: 'title',
            width: 250,
            ellipsis: {
                showTitle: false,
            },
            render: title => (
                <Tooltip placement="topLeft" title={title}>
                    <span>{title}</span>
                </Tooltip>
            ),
        },
        {
            title: '状态',
            dataIndex: 'status',
            key: 'status',
            width: 80,
            render: (status) => {
                const statusMap = {
                    0: { text: '草稿', color: 'default' },
                    1: { text: '已发布', color: 'success' },
                    2: { text: '已结束', color: 'error' },
                };
                const { text, color } = statusMap[status] || { text: '未知', color: 'default' };
                return <Tag color={color}>{text}</Tag>;
            },
        },
        {
            title: '开始时间',
            dataIndex: 'startTime',
            key: 'startTime',
            width: 100,
            render: (date) => new Date(date).toLocaleString(),
        },
        {
            title: '结束时间',
            dataIndex: 'endTime',
            key: 'endTime',
            width: 100,
            render: (date) => new Date(date).toLocaleString(),
        },
        {
            title: '操作',
            key: 'action',
            fixed: 'right',
            width: 340,
            render: (_, record) => {
                const isPublished = record.status === 1;
                const isDraft = record.status === 0;
                const isEnded = record.status === 2;

                // 创建状态切换的下拉菜单项
                const items = [];

                // 草稿状态 -> 可以发布
                if (isDraft) {
                    items.push({
                        key: 'publish',
                        label: '发布问卷',
                        icon: <CheckOutlined style={{ color: token.colorSuccess }} />,
                        onClick: () => handleStatusChange(record.id, 1)
                    });
                }

                // 已发布状态 -> 可以结束
                if (isPublished) {
                    items.push({
                        key: 'end',
                        label: '结束问卷',
                        icon: <CloseOutlined style={{ color: token.colorError }} />,
                        onClick: () => handleStatusChange(record.id, 2)
                    });
                }

                // 已结束状态 -> 可以重新发布
                if (isEnded) {
                    items.push({
                        key: 'publish',
                        label: '重新发布',
                        icon: <CheckOutlined style={{ color: token.colorSuccess }} />,
                        onClick: () => handleStatusChange(record.id, 1)
                    });
                }

                // 已发布或已结束状态 -> 可以复制为草稿
                if (isPublished || isEnded) {
                    items.push({
                        key: 'to-draft',
                        label: '复制为草稿',
                        icon: <FileTextOutlined style={{ color: token.colorInfo }} />,
                        onClick: () => handleStatusChange(record.id, 0)
                    });
                }

                return (
                    <Space size="small" style={{ flexWrap: 'nowrap' }}>
                        <Tooltip title="查看问卷中的问题列表" placement="top">
                            <Button
                                type="text"
                                icon={<QuestionCircleOutlined />}
                                onClick={() => navigate(`/surveys/${record.id}/questions`)}
                                style={{
                                    ...actionBtnStyle,
                                    color: token.colorInfo,
                                }}
                                className="action-btn question-btn"
                            >
                                问题
                            </Button>
                        </Tooltip>

                        <Tooltip title="预览完整问卷" placement="top">
                            <Button
                                type="text"
                                icon={<EyeOutlined />}
                                onClick={() => navigate(`/surveys/${record.id}/preview`)}
                                style={{
                                    ...actionBtnStyle,
                                    color: token.colorPrimary,
                                }}
                                className="action-btn preview-btn"
                            >
                                预览
                            </Button>
                        </Tooltip>

                        {isDraft && (
                            <Tooltip title="编辑问卷基本信息" placement="top">
                                <Button
                                    type="text"
                                    icon={<EditOutlined />}
                                    onClick={() => navigate(`/surveys/${record.id}/edit`)}
                                    style={{
                                        ...actionBtnStyle,
                                        color: token.colorWarning,
                                    }}
                                    className="action-btn edit-btn"
                                >
                                    编辑
                                </Button>
                            </Tooltip>
                        )}

                        {(isPublished || isEnded) && (
                            <>
                                <Tooltip title="分享问卷链接或二维码" placement="top">
                                    <Button
                                        type="text"
                                        icon={<ShareAltOutlined />}
                                        onClick={() => handleShare(record)}
                                        style={{
                                            ...actionBtnStyle,
                                            color: token.colorSuccess,
                                        }}
                                        className="action-btn share-btn"
                                    >
                                        分享
                                    </Button>
                                </Tooltip>

                                <Tooltip title="导出问卷回答数据" placement="top">
                                    <Button
                                        type="text"
                                        icon={<DownloadOutlined />}
                                        onClick={() => handleExport(record.id)}
                                        style={{
                                            ...actionBtnStyle,
                                            color: token.colorPrimary,
                                        }}
                                        className="action-btn export-btn"
                                    >
                                        导出
                                    </Button>
                                </Tooltip>
                            </>
                        )}

                        <Tooltip title="创建此问卷的副本" placement="top">
                            <Button
                                type="text"
                                icon={<CopyOutlined />}
                                onClick={() => handleCopy(record.id)}
                                style={{
                                    ...actionBtnStyle,
                                    color: token.colorInfo,
                                }}
                                className="action-btn copy-btn"
                            >
                                复制
                            </Button>
                        </Tooltip>

                        {/* 状态操作下拉菜单 */}
                        {items.length > 0 && (
                            <Tooltip title="更多操作" placement="top">
                                <Dropdown
                                    menu={{
                                        items,
                                        style: {
                                            borderRadius: '6px',
                                            boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)'
                                        }
                                    }}
                                    trigger={['click']}
                                    placement="bottomRight"
                                >
                                    <Button
                                        type="text"
                                        icon={<MoreOutlined />}
                                        style={{
                                            ...actionBtnStyle,
                                        }}
                                        className="action-btn more-btn"
                                    />
                                </Dropdown>
                            </Tooltip>
                        )}

                        {isDraft && (
                            <Tooltip title="永久删除此问卷" placement="top" color={token.colorError}>
                                <Button
                                    type="text"
                                    danger
                                    icon={<DeleteOutlined />}
                                    onClick={() => handleDelete(record.id)}
                                    style={{
                                        ...actionBtnStyle,
                                    }}
                                    className="action-btn delete-btn"
                                >
                                    删除
                                </Button>
                            </Tooltip>
                        )}
                    </Space>
                );
            },
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 16 }}>
                <Tooltip title="创建新的问卷" placement="left">
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={() => navigate('/surveys/create')}
                        style={{
                            borderRadius: '6px',
                            fontWeight: 500,
                            boxShadow: '0 2px 0 rgba(0, 0, 0, 0.045)'
                        }}
                    >
                        添加问卷
                    </Button>
                </Tooltip>
            </div>

            {surveys.length === 0 && !loading ? (
                <div style={{
                    textAlign: 'center',
                    padding: '40px 0',
                    background: '#fafafa',
                    borderRadius: '8px',
                    marginBottom: '16px',
                    boxShadow: '0 1px 2px rgba(0, 0, 0, 0.03)'
                }}>
                    <p style={{ color: token.colorTextSecondary, marginBottom: '16px' }}>暂无问卷数据</p>
                    <Tooltip title="开始创建您的第一个问卷" placement="top">
                        <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            onClick={() => navigate('/surveys/create')}
                            style={{
                                borderRadius: '6px',
                                fontWeight: 500
                            }}
                        >
                            创建第一个问卷
                        </Button>
                    </Tooltip>
                </div>
            ) : (
                <Table
                    columns={columns}
                    dataSource={surveys}
                    rowKey="id"
                    loading={loading}
                    pagination={pagination}
                    onChange={(pagination) => setPagination(pagination)}
                    scroll={{ x: 1100 }}
                    style={{
                        borderRadius: '8px',
                        overflow: 'hidden',
                        boxShadow: '0 1px 2px rgba(0, 0, 0, 0.03)'
                    }}
                    ref={tableRef}
                />
            )}

            {/* 分享问卷的弹窗 */}
            <Modal
                title={<div style={{ fontWeight: 600 }}>分享问卷</div>}
                open={shareModalVisible}
                onCancel={() => setShareModalVisible(false)}
                footer={[
                    <Button
                        key="close"
                        onClick={() => setShareModalVisible(false)}
                        style={{ borderRadius: '6px' }}
                    >
                        关闭
                    </Button>,
                    <Tooltip key="copy-tooltip" title="复制链接到剪贴板" placement="top">
                        <Button
                            key="copy"
                            type="primary"
                            icon={<CopyOutlined />}
                            onClick={copyShareLink}
                            style={{ borderRadius: '6px' }}
                        >
                            复制链接
                        </Button>
                    </Tooltip>
                ]}
                width={600}
                style={{ borderRadius: '8px', overflow: 'hidden', body: { padding: '16px 24px' } }}
                modalRender={(modal) => (
                    <div ref={modalRef}>
                        {modal}
                    </div>
                )}
            >
                <Tabs
                    activeKey={activeTab}
                    onChange={setActiveTab}
                    items={[
                        {
                            key: 'link',
                            label: (
                                <span>
                                    <LinkOutlined />
                                    链接分享
                                </span>
                            ),
                            children: (
                                <>
                                    <p style={{ marginBottom: '12px' }}>您可以通过以下链接分享此问卷给匿名参与者:</p>
                                    <Input
                                        readOnly
                                        value={shareLink}
                                        style={{ borderRadius: '6px' }}
                                        addonAfter={
                                            <Tooltip title="复制链接" placement="top">
                                                <Button
                                                    type="text"
                                                    icon={<CopyOutlined />}
                                                    onClick={copyShareLink}
                                                />
                                            </Tooltip>
                                        }
                                    />
                                </>
                            ),
                        },
                        {
                            key: 'qrcode',
                            label: (
                                <span>
                                    <QrcodeOutlined />
                                    二维码分享
                                </span>
                            ),
                            children: (
                                <div style={{ textAlign: 'center' }}>
                                    <p style={{ marginBottom: '12px' }}>扫描以下二维码参与问卷调查:</p>
                                    <div style={{
                                        margin: '20px 0',
                                        background: '#fff',
                                        padding: '16px',
                                        border: '1px solid #f0f0f0',
                                        borderRadius: '8px',
                                        display: 'inline-block'
                                    }}>
                                        <QRCodeSVG
                                            value={shareLink}
                                            size={200}
                                            level="H"
                                            includeMargin={true}
                                            imageSettings={{
                                                x: undefined,
                                                y: undefined,
                                                excavate: false
                                            }}
                                        />
                                    </div>
                                    <p>
                                        <Tooltip title="同时复制分享链接到剪贴板" placement="top">
                                            <Button
                                                type="primary"
                                                icon={<CopyOutlined />}
                                                onClick={copyShareLink}
                                                style={{ borderRadius: '6px' }}
                                            >
                                                同时复制链接
                                            </Button>
                                        </Tooltip>
                                    </p>
                                </div>
                            ),
                        },
                    ]}
                />

                <Divider style={{ margin: '16px 0' }} />

                <div style={{ marginTop: 16 }}>
                    <p style={{ margin: '8px 0' }}><strong>问卷标题:</strong> {currentSurvey?.title}</p>
                    <p style={{ margin: '8px 0' }}>
                        <strong>问卷状态:</strong> {
                            currentSurvey?.status === 1 ?
                                <Tag color="success">已发布</Tag> :
                                currentSurvey?.status === 2 ?
                                    <Tag color="error">已结束</Tag> :
                                    <Tag>草稿</Tag>
                        }
                    </p>
                    <p style={{
                        margin: '8px 0',
                        padding: '8px 12px',
                        background: currentSurvey?.status === 1 ? token.colorSuccessBg : token.colorErrorBg,
                        borderRadius: '6px'
                    }}>
                        <strong>注意:</strong> {
                            currentSurvey?.status === 1
                                ? '此问卷当前可以被作答。'
                                : '此问卷当前不可作答，请先将状态修改为"已发布"。'
                        }
                    </p>
                </div>
            </Modal>
        </div>
    );
};

export default SurveyList;
