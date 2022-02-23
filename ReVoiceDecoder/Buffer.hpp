#pragma once
#include <vector>  // buffers
#include <sstream> // strings, byteStr()

class Buffer {
public:
    Buffer() ;
    Buffer(const std::vector<unsigned char>&) ;

    void setBuffer(std::vector<unsigned char>&) ;
    const std::vector<unsigned char> &getBuffer() const ;
    void clear() ;

    std::string byteStr(bool LE = true) const ;

    /************************** Writing ***************************/

    template <class T> inline void writeBytes(const T &val, bool LE = true);
    unsigned long long getWriteOffset() const ;

    void writeBool(bool) ;
    void writeStr(const std::string&) ;
    void writeInt8(char) ;
    void writeUInt8(unsigned char) ;

    void writeInt16_LE(short) ;
    void writeInt16_BE(short) ;
    void writeUInt16_LE(unsigned short) ;
    void writeUInt16_BE(unsigned short) ;

    void writeInt32_LE(int) ;
    void writeInt32_BE(int) ;
    void writeUInt32_LE(unsigned int) ;
    void writeUInt32_BE(unsigned int) ;

    void writeFloat_LE(float) ;
    void writeFloat_BE(float) ;
    void writeDouble_LE(double) ;
    void writeDouble_BE(double) ;

    /************************** Reading ***************************/

    void setReadOffset(unsigned long long) ;
    unsigned long long getReadOffset() const ;
    template <class T> inline T readBytes(bool LE = true);

    bool               readBool() ;
    std::string        readStr(unsigned long long len) ;
    std::string        readStr() ;
    char               readInt8() ;
    unsigned char      readUInt8() ;

    short              readInt16_LE() ;
    short              readInt16_BE() ;
    unsigned short     readUInt16_LE() ;
    unsigned short     readUInt16_BE() ;

    int                readInt32_LE() ;
    int                readInt32_BE() ;
    unsigned int       readUInt32_LE() ;
    unsigned int       readUInt32_BE() ;

    float              readFloat_LE() ;
    float              readFloat_BE() ;
    double             readDouble_LE() ;
    double             readDouble_BE() ;

    ~Buffer();
private:
    std::vector<unsigned char> buffer;
    unsigned long long readOffset = 0;
    unsigned long long writeOffset = 0;
};
