import React, { useState, useEffect } from 'react';
import { Modal, Button, List, Typography, Space, Descriptions, Tag, Tooltip, Timeline, Spin, Empty, message } from 'antd';
import { 
    HistoryOutlined, 
    RollbackOutlined, 
    EyeOutlined, 
    ClockCircleOutlined, 
    CheckCircleOutlined,
    ExclamationCircleOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { contentApi } from './InfoPage';

const { Title, Text, Paragraph } = Typography;
const { confirm } = Modal;

/**
 * 版本历史组件
 * @param {Object} props - 组件属性
 * @param {string} props.contentId - 内容ID
 * @param {boolean} props.visible - 是否显示
 * @param {Function} props.onClose - 关闭回调
 * @param {Function} props.onRefresh - 刷新回调，回滚版本后需要刷新内容
 */
const VersionHistory = ({ contentId, visible, onClose, onRefresh }) => {
    const [versions, setVersions] = useState([]);
    const [selectedVersion, setSelectedVersion] = useState(null);
    const [versionDetailVisible, setVersionDetailVisible] = useState(false);
    const [loading, setLoading] = useState(false);

    // 加载版本历史
    useEffect(() => {
        if (visible && contentId) {
            fetchVersions();
        }
    }, [visible, contentId]);

    // 获取版本列表
    const fetchVersions = async () => {
        if (!contentId) return;
        
        setLoading(true);
        try {
            const response = await contentApi.getVersions(contentId);
            setVersions(response.data || []);
        } catch (error) {
            console.error('获取版本历史失败:', error);
            message.error('获取版本历史失败: ' + (error.response?.data?.message || error.message));
        } finally {
            setLoading(false);
        }
    };

    // 查看版本详情
    const showVersionDetail = async (versionId) => {
        setLoading(true);
        try {
            const response = await contentApi.getVersion(contentId, versionId);
            setSelectedVersion(response.data);
            setVersionDetailVisible(true);
        } catch (error) {
            console.error('获取版本详情失败:', error);
            message.error('获取版本详情失败: ' + (error.response?.data?.message || error.message));
        } finally {
            setLoading(false);
        }
    };

    // 回滚版本
    const handleRollback = (version) => {
        confirm({
            title: '确认回滚版本',
            icon: <ExclamationCircleOutlined />,
            content: `确定要将内容回滚到 ${dayjs(version.createdAt).format('YYYY-MM-DD HH:mm:ss')} 的版本吗？此操作不可撤销！`,
            okText: '确定回滚',
            cancelText: '取消',
            onOk: async () => {
                setLoading(true);
                try {
                    await contentApi.rollbackToVersion(contentId, version.id);
                    message.success('版本回滚成功');
                    if (onRefresh) onRefresh();
                    // 关闭弹窗
                    onClose();
                } catch (error) {
                    console.error('版本回滚失败:', error);
                    message.error('版本回滚失败: ' + (error.response?.data?.message || error.message));
                } finally {
                    setLoading(false);
                }
            }
        });
    };

    // 格式化日期
    const formatDate = (dateString) => {
        if (!dateString) return '-';
        return dayjs(dateString).format('YYYY-MM-DD HH:mm:ss');
    };

    // 渲染版本列表
    const renderVersionList = () => {
        if (loading && versions.length === 0) {
            return <Spin tip="加载版本历史中..." />;
        }

        if (versions.length === 0) {
            return <Empty description="暂无版本历史记录" />;
        }

        return (
            <Timeline mode="left">
                {versions.map((version, index) => (
                    <Timeline.Item 
                        key={version.id} 
                        color={index === 0 ? 'green' : 'blue'}
                        dot={index === 0 ? <CheckCircleOutlined /> : <ClockCircleOutlined />}
                    >
                        <Space direction="vertical" style={{ width: '100%' }}>
                            <Space align="center">
                                <Text strong>{formatDate(version.createdAt)}</Text>
                                {index === 0 && <Tag color="success">最新版本</Tag>}
                            </Space>
                            
                            <Paragraph type="secondary" ellipsis={{ rows: 2 }}>
                                {version.description || '无描述'}
                            </Paragraph>
                            
                            <Space>
                                <Tooltip title="查看详情">
                                    <Button
                                        type="text"
                                        icon={<EyeOutlined />}
                                        onClick={() => showVersionDetail(version.id)}
                                    >
                                        查看
                                    </Button>
                                </Tooltip>
                                
                                {index !== 0 && (
                                    <Tooltip title="回滚到此版本">
                                        <Button
                                            type="text"
                                            danger
                                            icon={<RollbackOutlined />}
                                            onClick={() => handleRollback(version)}
                                        >
                                            回滚
                                        </Button>
                                    </Tooltip>
                                )}
                            </Space>
                        </Space>
                    </Timeline.Item>
                ))}
            </Timeline>
        );
    };

    // 渲染版本详情
    const renderVersionDetail = () => {
        if (!selectedVersion) return null;

        return (
            <Modal
                title={
                    <Space>
                        <ClockCircleOutlined />
                        <span>版本详情</span>
                        <Tag color="blue">{formatDate(selectedVersion.createdAt)}</Tag>
                    </Space>
                }
                open={versionDetailVisible}
                onCancel={() => setVersionDetailVisible(false)}
                footer={[
                    <Button key="back" onClick={() => setVersionDetailVisible(false)}>
                        关闭
                    </Button>
                ]}
                width={700}
            >
                <Spin spinning={loading}>
                    <Descriptions bordered column={1}>
                        <Descriptions.Item label="版本ID">{selectedVersion.id}</Descriptions.Item>
                        <Descriptions.Item label="创建时间">{formatDate(selectedVersion.createdAt)}</Descriptions.Item>
                        <Descriptions.Item label="描述">{selectedVersion.description || '无描述'}</Descriptions.Item>
                        <Descriptions.Item label="内容">
                            <div style={{ maxHeight: '300px', overflow: 'auto', border: '1px solid #f0f0f0', padding: '8px', backgroundColor: '#fafafa' }}>
                                <pre style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
                                    {selectedVersion.content}
                                </pre>
                            </div>
                        </Descriptions.Item>
                    </Descriptions>
                </Spin>
            </Modal>
        );
    };

    return (
        <>
            <Modal
                title={
                    <Space>
                        <HistoryOutlined />
                        <span>版本历史</span>
                    </Space>
                }
                open={visible}
                onCancel={onClose}
                footer={[
                    <Button key="refresh" onClick={fetchVersions} loading={loading}>
                        刷新
                    </Button>,
                    <Button key="close" onClick={onClose}>
                        关闭
                    </Button>
                ]}
                width={700}
            >
                <Spin spinning={loading && versions.length > 0}>
                    {renderVersionList()}
                </Spin>
            </Modal>
            
            {renderVersionDetail()}
        </>
    );
};

export default VersionHistory;