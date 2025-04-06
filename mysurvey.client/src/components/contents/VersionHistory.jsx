import React, { useState, useEffect } from 'react';
import {
    Button,
    Modal,
    List,
    Typography,
    Space,
    Tag,
    Spin,
    Empty,
    Switch,
    message,
    Table,
    Descriptions,
    Divider,
    Alert,
    Card
} from 'antd';
import {
    HistoryOutlined,
    ExclamationCircleOutlined,
} from '@ant-design/icons';
import { theme } from 'antd';
import dayjs from 'dayjs';

const { Title, Text, Paragraph } = Typography;
const { useToken } = theme;

// 格式化日期工具函数
const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = dayjs(dateString);
    return date.isValid() ? date.format('YYYY-MM-DD HH:mm:ss') : '-';
};

// 版本历史组件
const VersionHistory = ({ contentId, api, onVersionRollback }) => {
    const [versions, setVersions] = useState([]);
    const [loading, setLoading] = useState(false);
    const [selectedVersion, setSelectedVersion] = useState(null);
    const [versionDetails, setVersionDetails] = useState(null);
    const [detailsLoading, setDetailsLoading] = useState(false);
    const [compareMode, setCompareMode] = useState(false);
    const [compareVersionId, setCompareVersionId] = useState(null);
    const [compareDetails, setCompareDetails] = useState(null);

    // 获取主题令牌，用于配色
    const { token } = useToken();

    // 获取版本历史
    useEffect(() => {
        if (!contentId) return;

        const fetchVersions = async () => {
            setLoading(true);
            try {
                const response = await api.getVersions(contentId);
                if (response.data && Array.isArray(response.data)) {
                    setVersions(response.data);
                }
            } catch (error) {
                console.error('获取版本历史失败:', error);
                message.error('获取版本历史失败');
            } finally {
                setLoading(false);
            }
        };

        fetchVersions();
    }, [contentId, api]);

    // 获取版本详情
    const fetchVersionDetails = async (versionId) => {
        if (!contentId || !versionId) return;

        setDetailsLoading(true);
        try {
            const response = await api.getVersion(contentId, versionId);
            if (response.data) {
                setVersionDetails(response.data);
            }
        } catch (error) {
            console.error('获取版本详情失败:', error);
            message.error('获取版本详情失败');
        } finally {
            setDetailsLoading(false);
        }
    };

    // 获取比较版本详情
    const fetchCompareDetails = async (versionId) => {
        if (!contentId || !versionId) return;

        setDetailsLoading(true);
        try {
            const response = await api.getVersion(contentId, versionId);
            if (response.data) {
                setCompareDetails(response.data);
            }
        } catch (error) {
            console.error('获取比较版本详情失败:', error);
            message.error('获取比较版本详情失败');
        } finally {
            setDetailsLoading(false);
        }
    };

    // 处理版本选择
    const handleVersionSelect = async (versionId) => {
        setSelectedVersion(versionId);
        setCompareMode(false);
        setCompareVersionId(null);
        setCompareDetails(null);
        await fetchVersionDetails(versionId);
    };

    // 启用比较模式
    const handleCompareMode = (enable) => {
        setCompareMode(enable);
        if (!enable) {
            setCompareVersionId(null);
            setCompareDetails(null);
        }
    };

    // 处理比较版本选择
    const handleCompareVersionSelect = async (versionId) => {
        setCompareVersionId(versionId);
        await fetchCompareDetails(versionId);
    };

    // 处理回滚到某个版本
    const handleRollback = async (versionId) => {
        Modal.confirm({
            title: '确认回滚',
            icon: <ExclamationCircleOutlined />,
            content: '确定要回滚到这个版本吗？这将覆盖当前内容！',
            okText: '确认回滚',
            cancelText: '取消',
            onOk: async () => {
                try {
                    setLoading(true);
                    await api.rollbackToVersion(contentId, versionId);
                    message.success('回滚成功');

                    // 通知父组件刷新数据
                    if (onVersionRollback) {
                        onVersionRollback();
                    }
                } catch (error) {
                    console.error('回滚失败:', error);
                    message.error('回滚失败: ' + (error.response?.data?.message || error.message));
                } finally {
                    setLoading(false);
                }
            }
        });
    };

    // 渲染JSON内容差异
    const renderContentDiff = () => {
        if (!versionDetails || !compareDetails) return null;

        try {
            // 解析JSON内容
            const versionContent = typeof versionDetails.jsonContent === 'string'
                ? JSON.parse(versionDetails.jsonContent)
                : versionDetails.jsonContent;

            const compareContent = typeof compareDetails.jsonContent === 'string'
                ? JSON.parse(compareDetails.jsonContent)
                : compareDetails.jsonContent;

            // 创建差异对象
            const diffItems = [];

            // 处理数组类型的内容
            if (Array.isArray(versionContent) && Array.isArray(compareContent)) {
                // 创建字段映射
                const versionMap = versionContent.reduce((map, item) => {
                    map[item.fieldName] = item.value;
                    return map;
                }, {});

                const compareMap = compareContent.reduce((map, item) => {
                    map[item.fieldName] = item.value;
                    return map;
                }, {});

                // 收集所有字段名
                const allFields = [...new Set([
                    ...versionContent.map(item => item.fieldName),
                    ...compareContent.map(item => item.fieldName)
                ])];

                // 比较每个字段
                allFields.forEach(fieldName => {
                    const versionValue = versionMap[fieldName];
                    const compareValue = compareMap[fieldName];

                    if (JSON.stringify(versionValue) !== JSON.stringify(compareValue)) {
                        diffItems.push({
                            fieldName,
                            versionValue: versionValue !== undefined ? versionValue : '(无)',
                            compareValue: compareValue !== undefined ? compareValue : '(无)'
                        });
                    }
                });
            } else {
                // 处理对象类型的内容
                const allKeys = [...new Set([
                    ...Object.keys(versionContent || {}),
                    ...Object.keys(compareContent || {})
                ])];

                allKeys.forEach(key => {
                    const versionValue = versionContent?.[key];
                    const compareValue = compareContent?.[key];

                    if (JSON.stringify(versionValue) !== JSON.stringify(compareValue)) {
                        diffItems.push({
                            fieldName: key,
                            versionValue: versionValue !== undefined ? versionValue : '(无)',
                            compareValue: compareValue !== undefined ? compareValue : '(无)'
                        });
                    }
                });
            }

            // 渲染差异表格
            return (
                <div style={{ marginTop: 16 }}>
                    <Divider>内容差异比较</Divider>
                    {diffItems.length > 0 ? (
                        <Table
                            dataSource={diffItems}
                            rowKey="fieldName"
                            pagination={false}
                            size="small"
                            columns={[
                                {
                                    title: '字段',
                                    dataIndex: 'fieldName',
                                    width: 150,
                                },
                                {
                                    title: `版本 ${compareDetails.version || '未知'}`,
                                    dataIndex: 'compareValue',
                                    render: (value) => (
                                        <div
                                            style={{
                                                background: token.colorErrorBg,
                                                padding: '4px 8px',
                                                borderRadius: '4px'
                                            }}
                                        >
                                            {typeof value === 'object'
                                                ? JSON.stringify(value, null, 2)
                                                : String(value)}
                                        </div>
                                    )
                                },
                                {
                                    title: `版本 ${versionDetails.version || '未知'}`,
                                    dataIndex: 'versionValue',
                                    render: (value) => (
                                        <div
                                            style={{
                                                background: token.colorSuccessBg,
                                                padding: '4px 8px',
                                                borderRadius: '4px'
                                            }}
                                        >
                                            {typeof value === 'object'
                                                ? JSON.stringify(value, null, 2)
                                                : String(value)}
                                        </div>
                                    )
                                },
                            ]}
                        />
                    ) : (
                        <Empty description="两个版本内容相同，没有差异" />
                    )}
                </div>
            );
        } catch (error) {
            console.error('渲染差异失败:', error);
            return (
                <Alert
                    type="error"
                    message="差异比较失败"
                    description="无法解析内容进行比较，请确保内容格式正确"
                />
            );
        }
    };

    // 渲染版本详情
    const renderVersionDetails = () => {
        if (!versionDetails) return null;

        return (
            <Card
                title={
                    <Space>
                        <span>版本 {versionDetails.version || '未知'} 详情</span>
                        <Tag color="blue">{formatDate(versionDetails.createdAt)}</Tag>
                    </Space>
                }
                extra={
                    <Button
                        type="primary"
                        danger
                        onClick={() => handleRollback(selectedVersion)}
                    >
                        回滚到此版本
                    </Button>
                }
                style={{ marginTop: 16 }}
            >
                <Descriptions bordered size="small" column={2}>
                    <Descriptions.Item label="状态">
                        {versionDetails.status === 1 ? (
                            <Tag color="success">已发布</Tag>
                        ) : (
                            <Tag>草稿</Tag>
                        )}
                    </Descriptions.Item>
                    <Descriptions.Item label="创建时间">{formatDate(versionDetails.createdAt)}</Descriptions.Item>
                </Descriptions>

                <Divider orientation="left">内容数据</Divider>
                <Typography.Paragraph>
                    <pre style={{
                        maxHeight: '300px',
                        overflow: 'auto',
                        background: token.colorBgContainer,
                        padding: 16,
                        borderRadius: 4,
                        border: `1px solid ${token.colorBorder}`
                    }}>
                        {JSON.stringify(
                            typeof versionDetails.snapshot === 'string'
                                ? JSON.parse(versionDetails.snapshot)
                                : versionDetails.snapshot,
                            null, 2
                        )}
                    </pre>
                </Typography.Paragraph>
            </Card>
        );
    };

    return (
        <Spin spinning={loading} tip="加载版本历史...">
            <div style={{ display: 'flex', marginBottom: 16 }}>
                <Title level={5} style={{ margin: 0, marginRight: 'auto' }}>
                    <HistoryOutlined /> 版本历史记录
                </Title>
                {selectedVersion && (
                    <Space>
                        <Switch
                            checkedChildren="比较模式"
                            unCheckedChildren="详情模式"
                            checked={compareMode}
                            onChange={handleCompareMode}
                        />
                    </Space>
                )}
            </div>

            {versions.length === 0 ? (
                <Empty description="暂无版本历史记录" />
            ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                    <List
                        size="small"
                        bordered
                        dataSource={versions}
                        renderItem={(version) => (
                            <List.Item
                                key={version.id}
                                style={{
                                    cursor: 'pointer',
                                    background: selectedVersion === version.id
                                        ? token.colorPrimaryBg
                                        : (compareVersionId === version.id
                                            ? token.colorInfoBg
                                            : 'transparent')
                                }}
                                onClick={() => {
                                    if (compareMode && selectedVersion !== version.id) {
                                        handleCompareVersionSelect(version.id);
                                    } else if (!compareMode) {
                                        handleVersionSelect(version.id);
                                    }
                                }}
                                actions={[
                                    <Button
                                        key="rollback"
                                        type="link"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleRollback(version.id);
                                        }}
                                    >
                                        回滚
                                    </Button>
                                ]}
                            >
                                <Space>
                                    <Tag color="blue">版本 {version.version || '未知'}</Tag>
                                    <span>{formatDate(version.createdAt)}</span>
                                    {version.createdBy && (
                                        <span>由 {version.createdBy} 创建</span>
                                    )}
                                    {version.status === 1 ? (
                                        <Tag color="success">已发布</Tag>
                                    ) : (
                                        <Tag>草稿</Tag>
                                    )}
                                </Space>
                            </List.Item>
                        )}
                    />

                    <Spin spinning={detailsLoading} tip="加载版本详情...">
                        {compareMode && selectedVersion && compareVersionId ? (
                            renderContentDiff()
                        ) : (
                            selectedVersion && renderVersionDetails()
                        )}
                    </Spin>
                </div>
            )}
        </Spin>
    );
};

// 版本历史模态框组件
export const VersionHistoryModal = ({ contentId, api, visible, onClose, onVersionRollback }) => {
    return (
        <Modal
            title="版本历史"
            open={visible}
            onCancel={onClose}
            width={900}
            footer={[
                <Button key="close" onClick={onClose}>
                    关闭
                </Button>
            ]}
        >
            <VersionHistory
                contentId={contentId}
                api={api}
                onVersionRollback={onVersionRollback}
            />
        </Modal>
    );
};

export default VersionHistory;